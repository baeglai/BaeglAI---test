public interface ICustomerOrderCollectorService
{
    Task<(Order order, Customer customer)> SaveOrderAndCustomerFromCall(string callId);
}
