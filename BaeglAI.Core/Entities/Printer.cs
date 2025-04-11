public class Printer
{
    public string PrinterId { get; set; }
    public PrinterLocation Location { get; set; }
    public string PrintNodePrinterId { get; set; }
    public PrinterStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}