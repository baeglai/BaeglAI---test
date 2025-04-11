public class CallOrderDto
{
    public string CallId { get; set; }
    public string CustomerName { get; set; }
    public string PhoneNumber { get; set; }
    public List<OrderItem> Items { get; set; }
    public string Transcript { get; set; }
    public string RecordingUrl { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime OrderDate { get; set; }
}
