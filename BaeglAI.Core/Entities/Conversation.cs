public class Conversation
{
    public string ConversationId { get; set; }
    public string StoreId { get; set; }
    public string CustomerId { get; set; } // optional
    public DateTime Timestamp { get; set; }
    public string RecordingUrl { get; set; }
    public string Transcript { get; set; }
    public int Duration { get; set; } // seconds
    public string CallerPhoneNumber { get; set; }
    public ConversationStatus Status { get; set; }
}