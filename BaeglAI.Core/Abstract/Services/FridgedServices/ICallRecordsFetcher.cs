using System.Text.Json;

public interface ICallRecordsFetcher
{
    Task<JsonElement> StructuredDataFetch(string callId);
}