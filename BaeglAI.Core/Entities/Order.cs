public class Order
{
    public string OrderId { get; set; }
    public string CustomerId { get; set; }
    public string CustomerName { get; set; }       // e.g. "Ata"
    public string PhoneNumber { get; set; }        // e.g. "+16313813076"
    public List<OrderItem> Items { get; set; }
    public decimal TotalPrice { get; set; }
    public string Note { get; set; }               // Transcript
    public DateTime OrderDate { get; set; }
    public int Status { get; set; }
    public string RecordingUrl { get; set; }       // VAPI'den gelen kayıt linki
}
