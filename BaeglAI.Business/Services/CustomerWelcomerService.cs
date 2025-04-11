using System.Net.Http.Headers;
using System.Text.Json;

public class CustomerWelcomerService : ICustomerWelcomerService
{
    private readonly HttpClient _httpClient;
    private readonly IRepository<Order> _orderRepository;
    private readonly string _storeId = "default-store-id";

    public CustomerWelcomerService(HttpClient httpClient, IRepository<Order> orderRepository)
    {
        _httpClient = httpClient;
        _orderRepository = orderRepository;
    }

        public async Task<CallOrderDto> BuildOrderFromVapiCall(string callId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.vapi.ai/call/{callId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "ee36dff7-c05c-4aa8-a421-641d170d85b0");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(json).RootElement;

        var structured = root.GetProperty("analysis").GetProperty("structuredData");

        // 🔐 Telefon numarası çıkarımı (güvenli ve fallback destekli)
        string? phone = null;

        // 1. root.customer.number varsa al
        if (root.TryGetProperty("customer", out var customerProp) &&
            customerProp.TryGetProperty("number", out var numberProp))
        {
            phone = numberProp.GetString();
        }

        // 2. structuredData.customerPhone varsa fallback olarak al
        if (string.IsNullOrWhiteSpace(phone) &&
            structured.TryGetProperty("customerPhone", out var fallbackPhoneProp))
        {
            var fallback = fallbackPhoneProp.GetString();
            if (!string.IsNullOrWhiteSpace(fallback) && fallback.ToLower() != "unknown")
            {
                phone = fallback;
            }
        }

        // 3. Hâlâ null veya boşsa → test için GUID üret
        if (string.IsNullOrWhiteSpace(phone))
        {
            phone = $"test-{Guid.NewGuid()}";
        }

        // 🔁 Geri kalan kısımlar:
        var items = new List<OrderItem>();
        foreach (var item in structured.GetProperty("items").EnumerateArray())
        {
            items.Add(new OrderItem
            {
                Name = item.GetProperty("name").GetString(),
                Quantity = item.GetProperty("quantity").GetInt32(),
                UnitPrice = (double)(item.TryGetProperty("unitPrice", out var priceProp)
                    ? priceProp.GetDecimalOrDefault()
                    : 0m)
            });
        }

        return new CallOrderDto
        {
            CallId = root.GetProperty("id").GetString(),
            CustomerName = structured.GetProperty("customerName").GetStringOrNull(),
            PhoneNumber = phone,
            Items = items,
            Transcript = structured.GetProperty("transcript").GetStringOrNull(),
            RecordingUrl = root.GetProperty("recordingUrl").GetStringOrNull(),
            TotalPrice = structured.GetProperty("totalPrice").GetDecimalOrDefault(),
            OrderDate = root.GetProperty("startedAt").GetDateTimeOrDefault()
        };
    }


    public async Task<Order> SaveOrderFromCall(string callId)
    {
        var dto = await BuildOrderFromVapiCall(callId);

        if (string.IsNullOrWhiteSpace(dto.CustomerName))
            throw new Exception("Müşteri adı alınamadı, sipariş kaydedilemiyor.");

        var phone = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber;
        var safeCustomerId = string.IsNullOrWhiteSpace(phone) ? $"test-{Guid.NewGuid()}" : phone;


        var order = new Order
        {
            OrderId = dto.CallId,
            CustomerId = safeCustomerId,
            CustomerName = dto.CustomerName,
            PhoneNumber = phone,
            Items = dto.Items,
            TotalPrice = dto.TotalPrice,
            Note = dto.Transcript,
            OrderDate = dto.OrderDate,
            Status = (int)OrderStatus.Pending,
            RecordingUrl = dto.RecordingUrl
        };


        var success = await _orderRepository.UpdateAsync(_storeId, order.OrderId, order);

        if (!success)
            throw new Exception("Sipariş kaydedilemedi.");

        return order;
    }
}


public static class JsonElementExtensions
{
    public static decimal GetDecimalOrDefault(this JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out var value))
            return value;
        return 0;
    }

    public static DateTime GetDateTimeOrDefault(this JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String && element.TryGetDateTime(out var dt))
            return dt;
        return DateTime.MinValue;
    }

    public static string GetStringOrNull(this JsonElement element)
    {
        return element.ValueKind == JsonValueKind.String ? element.GetString() : null;
    }
}
