using System.Net.Http.Headers;
using System.Text.Json;

public class CustomerOrderCollectorService : ICustomerOrderCollectorService
{
    private readonly HttpClient _httpClient;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly string _storeId = "default-store-id";

    public CustomerOrderCollectorService(
        HttpClient httpClient,
        IRepository<Order> orderRepository,
        IRepository<Customer> customerRepository)
    {
        _httpClient = httpClient;
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
    }

    public async Task<(Order order, Customer customer)> SaveOrderAndCustomerFromCall(string callId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.vapi.ai/call/{callId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "ee36dff7-c05c-4aa8-a421-641d170d85b0");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(json).RootElement;

        if (!root.TryGetProperty("analysis", out var analysis) ||
            !analysis.TryGetProperty("structuredData", out var structured))
        {
            throw new Exception("structuredData bulunamadı.");
        }

        string? phone = null;

        if (root.TryGetProperty("customer", out var customerProp) &&
            customerProp.TryGetProperty("number", out var numberProp))
        {
            phone = numberProp.GetString();
        }

        if (string.IsNullOrWhiteSpace(phone) &&
            structured.TryGetProperty("customerPhone", out var fallbackPhoneProp))
        {
            var fallback = fallbackPhoneProp.GetString();
            if (!string.IsNullOrWhiteSpace(fallback) && fallback.ToLower() != "unknown")
            {
                phone = fallback;
            }
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            phone = $"test-{Guid.NewGuid()}";
        }

var items = new List<OrderItem>();
if (structured.TryGetProperty("items", out var itemsProp))
{
    foreach (var item in itemsProp.EnumerateArray())
    {
        items.Add(new OrderItem
        {
            Name = item.TryGetProperty("name", out var itemNameProp) ? itemNameProp.GetString() ?? "" : "",
            Quantity = item.TryGetProperty("quantity", out var qtyProp) ? qtyProp.GetInt32() : 1,
            UnitPrice = (double)(item.TryGetProperty("unitPrice", out var priceProp)
                ? priceProp.GetDecimalOrDefault()
                : 0m)
        });
    }
}

        var customer = new Customer
        {
            CustomerId = phone,
            PhoneNumber = phone,
            FirstName = structured.TryGetProperty("customerName", out var nameProp)
                ? nameProp.GetString() ?? "Unknown"
                : "Unknown",
            LastName = "",
            CreatedAt = DateTime.UtcNow,
            Notes = structured.TryGetProperty("transcript", out var transcriptProp)
                ? transcriptProp.GetString()
                : null,
            IsTemporary = false
        };

        var order = new Order
        {
            OrderId = root.TryGetProperty("id", out var idProp)
                ? idProp.GetString() ?? Guid.NewGuid().ToString()
                : Guid.NewGuid().ToString(),

            CustomerId = phone,
            CustomerName = customer.FirstName,
            PhoneNumber = phone,
            Items = items,
            TotalPrice = structured.TryGetProperty("totalPrice", out var totalPriceProp)
                ? totalPriceProp.GetDecimalOrDefault()
                : 0m,
            Note = customer.Notes,
            OrderDate = root.TryGetProperty("startedAt", out var dateProp)
                ? dateProp.GetDateTimeOrDefault()
                : DateTime.UtcNow,
            Status = (int)OrderStatus.Pending,
            RecordingUrl = root.TryGetProperty("recordingUrl", out var recordingProp)
                ? recordingProp.GetString()
                : null
        };

        var orderSuccess = await _orderRepository.UpdateAsync(_storeId, order.OrderId, order);
        var customerSuccess = await _customerRepository.UpdateAsync(_storeId, customer.CustomerId, customer);

        if (!orderSuccess || !customerSuccess)
            throw new Exception("Sipariş veya müşteri kaydedilemedi.");

        return (order, customer);
    }
}
