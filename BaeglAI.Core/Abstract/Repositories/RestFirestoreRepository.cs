using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

public static class FirestoreHelper
{
 public static object ToFirestoreDocument<T>(T entity)
    {
        var json = JsonSerializer.SerializeToNode(entity)!;
        var fields = new JsonObject();

        foreach (var kvp in json.AsObject())
        {
            fields[kvp.Key] = SerializeValue(kvp.Value);
        }

        return fields;
    }

    private static JsonObject SerializeValue(JsonNode? value)
    {
        var field = new JsonObject();

        switch (value)
        {
            case JsonValue v:
                return HandlePrimitive(v);

            case JsonArray array:
                field["arrayValue"] = new JsonObject
                {
                    ["values"] = new JsonArray(array.Select(SerializeValue).ToArray())
                };
                return field;

            case JsonObject obj:
                var nested = new JsonObject();
                foreach (var kvp in obj)
                {
                    nested[kvp.Key] = SerializeValue(kvp.Value);
                }

                field["mapValue"] = new JsonObject { ["fields"] = nested };
                return field;

            default:
                field["nullValue"] = "null";
                return field;
        }
    }

    private static JsonObject HandlePrimitive(JsonValue v)
    {
        var field = new JsonObject();
        switch (v.GetValueKind())
        {
            case JsonValueKind.String:
                field["stringValue"] = v.ToString();
                break;
            case JsonValueKind.Number:
                if (v.TryGetValue(out int intVal))
                    field["integerValue"] = intVal.ToString();
                else if (v.TryGetValue(out double dblVal))
                    field["doubleValue"] = dblVal.ToString();
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                field["booleanValue"] = v.ToString().ToLower();
                break;
            case JsonValueKind.Null:
                field["nullValue"] = "null";
                break;
        }
        return field;
    }

    public static T FromFirestoreDocument<T>(JsonElement doc)
    {
        var fields = doc.GetProperty("fields");
        var dict = new Dictionary<string, object>();

        foreach (var prop in fields.EnumerateObject())
        {
            dict[prop.Name] = ExtractValue(prop.Value);
        }

        var serialized = JsonSerializer.Serialize(dict);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        return JsonSerializer.Deserialize<T>(serialized, options)!;
    }

    private static object? ExtractValue(JsonElement element)
    {
        if (element.TryGetProperty("stringValue", out var s)) return s.GetString();
        if (element.TryGetProperty("integerValue", out var i)) return int.Parse(i.GetString()!);
        if (element.TryGetProperty("doubleValue", out var d)) return double.Parse(d.GetString()!);
        if (element.TryGetProperty("booleanValue", out var b)) return b.GetBoolean();
        if (element.TryGetProperty("nullValue", out _)) return null;

        if (element.TryGetProperty("mapValue", out var m))
        {
            var inner = new Dictionary<string, object>();
            if (m.TryGetProperty("fields", out var fields))
            {
                foreach (var f in fields.EnumerateObject())
                {
                    inner[f.Name] = ExtractValue(f.Value);
                }
            }
            return inner;
        }

        if (element.TryGetProperty("arrayValue", out var a))
        {
            var list = new List<object>();
            if (a.TryGetProperty("values", out var values))
            {
                foreach (var v in values.EnumerateArray())
                {
                    list.Add(ExtractValue(v));
                }
            }
            return list;
        }

        return null;
    }
}

public class RestFirestoreRepository<T> : IRepository<T> where T : class
{
   private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _collectionPath;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public RestFirestoreRepository(HttpClient httpClient, string baseUrl, string collectionPath)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
        _collectionPath = collectionPath;
    }

    private string GetDocumentUrl(string storeId, string documentId = "")
    {
        return $"{_baseUrl}/stores/{storeId}/{_collectionPath}" + (string.IsNullOrEmpty(documentId) ? "" : $"/{documentId}");
    }

    public async Task<bool> AddAsync(string storeId, T entity)
    {
        var url = GetDocumentUrl(storeId);
        var response = await _httpClient.PostAsJsonAsync(url, new { fields = FirestoreHelper.ToFirestoreDocument(entity) });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(string storeId, string entityId, T entity)
    {
        var url = GetDocumentUrl(storeId, entityId);
        var response = await _httpClient.PatchAsync(url, JsonContent.Create(new { fields = FirestoreHelper.ToFirestoreDocument(entity) }));
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(string storeId, string entityId)
    {
        var url = GetDocumentUrl(storeId, entityId);
        var response = await _httpClient.DeleteAsync(url);
        return response.IsSuccessStatusCode;
    }

    public async Task<T> GetByIdAsync(string storeId, string entityId)
    {
        var url = GetDocumentUrl(storeId, entityId);
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null!;

        var json = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(json);
        return FirestoreHelper.FromFirestoreDocument<T>(document.RootElement);
    }

    public async Task<IEnumerable<T>> GetAllAsync(string storeId)
    {
        var url = GetDocumentUrl(storeId);
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return Enumerable.Empty<T>();

        var json = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(json);

        if (!document.RootElement.TryGetProperty("documents", out var docs))
            return Enumerable.Empty<T>();

        return docs.EnumerateArray().Select(doc => FirestoreHelper.FromFirestoreDocument<T>(doc));
    }

    public async Task<IEnumerable<T>> GetWhereAsync(string storeId, Expression<Func<T, bool>> predicate)
    {
        var all = await GetAllAsync(storeId);
        return all.AsQueryable().Where(predicate).ToList();
    }
}
