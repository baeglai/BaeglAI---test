public class Store
{
    public string StoreId { get; set; }
    public string OwnerId { get; set; }
    public string Name { get; set; }
    public Address Address { get; set; }
    public string Website { get; set; } // optional
    public Dictionary<string, WorkingHour> WorkingHours { get; set; } = new();
    public PosSystem? PosSystem { get; set; } // optional
    public DateTime CreatedAt { get; set; }
    public string AssistantPhoneNumber { get; set; }  // 🔑 ÖNEMLİ ALAN
    
}