using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Text.Json;

public class GenericRepository<T> : IRepository<T> where T : class
{
    private readonly FirebaseContext _context;
    private readonly string _collectionPath;

    public GenericRepository(FirebaseContext context, string collectionPath)
    {
        _context = context;
        _collectionPath = collectionPath;
    }

    private string GetDocumentUrl(string storeId, string entityId = "")
    {
        return $"{_context.BaseUrl}/stores/{storeId}/{_collectionPath}" + (string.IsNullOrEmpty(entityId) ? "" : $"/{entityId}");
    }

    public async Task<bool> AddAsync(string storeId, T entity)
    {
        var url = GetDocumentUrl(storeId);
        var response = await _context.HttpClient.PostAsJsonAsync(url, new
        {
            fields = FirestoreHelper.ToFirestoreDocument(entity)
        });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(string storeId, string entityId, T entity)
    {
        var url = GetDocumentUrl(storeId, entityId);
        var response = await _context.HttpClient.PatchAsync(url, JsonContent.Create(new
        {
            fields = FirestoreHelper.ToFirestoreDocument(entity)
        }));
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(string storeId, string entityId)
    {
        var url = GetDocumentUrl(storeId, entityId);
        var response = await _context.HttpClient.DeleteAsync(url);
        return response.IsSuccessStatusCode;
    }

    public async Task<T> GetByIdAsync(string storeId, string entityId)
    {
        var url = GetDocumentUrl(storeId, entityId);
        var response = await _context.HttpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return default!;
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return FirestoreHelper.FromFirestoreDocument<T>(doc.RootElement);
    }

    public async Task<IEnumerable<T>> GetAllAsync(string storeId)
    {
        var url = GetDocumentUrl(storeId);
        var response = await _context.HttpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return Enumerable.Empty<T>();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("documents", out var documents))
            return Enumerable.Empty<T>();

        return documents.EnumerateArray()
            .Select(FirestoreHelper.FromFirestoreDocument<T>);
    }

    public async Task<IEnumerable<T>> GetWhereAsync(string storeId, Expression<Func<T, bool>> predicate)
    {
        var all = await GetAllAsync(storeId);
        return all.AsQueryable().Where(predicate);
    }
}
