public class CreateOrderDto
{
    public string CustomerId { get; set; } // Müşteri ID
    public string CustomerName { get; set; } // Müşteri adı
    public string CustomerPhone { get; set; } // Müşteri telefon numarası
    public List<OrderItemDto> Items { get; set; } // Sipariş öğeleri
    public string Note { get; set; } // Sipariş notu
}
