namespace BaeglAI.Business.Services;

public class OrderService
{
    private readonly IRepository<Order> _orderRepository;

    public OrderService(IRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync(string storeId)
    {
        var orders = await _orderRepository.GetAllAsync(storeId);

        return orders.Select(o => new OrderDto
        {
            OrderId = o.OrderId,
            CustomerId = o.CustomerId,
            Items = o.Items.Select(i => new OrderItemDto
            {
                Name = i.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            TotalPrice = (double)o.TotalPrice,
            Note = o.Note,
            OrderDate = o.OrderDate,
            Status = o.Status.ToString()
        });
    }

    public async Task<OrderDto?> GetByIdAsync(string storeId, string orderId)
    {
        var order = await _orderRepository.GetByIdAsync(storeId, orderId);
        if (order == null) return null;

        return new OrderDto
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Name = i.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            TotalPrice = (double)order.TotalPrice,
            Note = order.Note,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString()
        };
    }


    public async Task<bool> CreateAsync(string storeId, CreateOrderDto dto)
{
    var order = new Order
    {
        OrderId = Guid.NewGuid().ToString(), // Bu artık hem OrderId hem documentId olarak kullanılacak
        CustomerId = dto.CustomerId,
        Items = dto.Items.Select(i => new OrderItem
        {
            Name = i.Name,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList(),
        TotalPrice = (decimal)dto.Items.Sum(i => i.UnitPrice * i.Quantity),
        Note = dto.Note,
        OrderDate = DateTime.UtcNow,
        Status = (int)OrderStatus.Pending
    };

    // Belge ID olarak OrderId kullanılacak
    return await _orderRepository.UpdateAsync(storeId, order.OrderId, order);
}

    // public Task<bool> UpdateAsync(string storeId, string orderId, Order order)
    //     => _orderRepository.UpdateAsync(storeId, orderId, order);

    public Task<bool> UpdateAsync(string storeId, string orderId, Order order)
{
    order.OrderId = orderId; // Belge ID ile senkron
    return _orderRepository.UpdateAsync(storeId, orderId, order);
}


    public Task<bool> DeleteAsync(string storeId, string orderId)
        => _orderRepository.DeleteAsync(storeId, orderId);

    public async Task<IEnumerable<OrderDto>> GetByCustomerIdAsync(string storeId, string customerId)
    {
        var orders = await _orderRepository.GetAllAsync(storeId);
        var filtered = orders.Where(o => o.CustomerId == customerId);

        return filtered.Select(o => new OrderDto
        {
            OrderId = o.OrderId,
            CustomerId = o.CustomerId,
            Items = o.Items.Select(i => new OrderItemDto
            {
                Name = i.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            TotalPrice = (double)o.TotalPrice,
            Note = o.Note,
            OrderDate = o.OrderDate,
            Status = o.Status.ToString()
        });
    }
}
