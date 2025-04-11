public class OrderDto
{
    public string OrderId { get; set; }
    public string CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; }
    public double TotalPrice { get; set; }
    public string Note { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; }
}