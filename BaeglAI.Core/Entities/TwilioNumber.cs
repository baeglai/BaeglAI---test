public class TwilioNumber
{
    public string TwilioNumberId { get; set; }
    public string PhoneNumber { get; set; } // E.164 format
    public TwilioNumberStatus Status { get; set; }
    public DateTime? AssignedAt { get; set; } // optional
}