public class StoreLookupService : IStoreLookupService
{
    private readonly IRepository<Store> _storeRepository;

    public StoreLookupService(IRepository<Store> storeRepository)
    {
        _storeRepository = storeRepository;
    }

    public async Task<string?> GetStoreIdByAssistantNumber(string assistantPhoneNumber)
    {
        var stores = await _storeRepository.GetWhereAsync("*", s => s.AssistantPhoneNumber == assistantPhoneNumber);
        return stores.FirstOrDefault()?.StoreId;
    }
}
