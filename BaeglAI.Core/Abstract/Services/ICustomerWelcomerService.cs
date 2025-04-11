public interface ICustomerWelcomerService
{
    Task<CallOrderDto> BuildOrderFromVapiCall(string callId);
    Task<Order> SaveOrderFromCall(string callId);
}
