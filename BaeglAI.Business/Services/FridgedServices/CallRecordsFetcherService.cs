using System.Net.Http.Headers;
using System.Text.Json;

public class CallRecordsFetcher : ICallRecordsFetcher
{
    private readonly HttpClient _httpClient;

    public CallRecordsFetcher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<JsonElement> StructuredDataFetch(string callId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.vapi.ai/call/{callId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "ee36dff7-c05c-4aa8-a421-641d170d85b0");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(json);

        return document.RootElement.GetProperty("analysis").GetProperty("structuredData");
    }
}