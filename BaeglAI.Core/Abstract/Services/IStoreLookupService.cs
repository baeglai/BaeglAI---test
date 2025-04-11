public interface IStoreLookupService
{
    Task<string?> GetStoreIdByAssistantNumber(string assistantPhoneNumber);
}
