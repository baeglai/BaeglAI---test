using System.Text.Json.Serialization;

public class Customer
{
    public string CustomerId { get; set; } // phone number veya test-id
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string PhoneNumber { get; set; } // eşleşme için kullanılıyor

    public string Notes { get; set; } // Özel notlar, tercih vs.
    public DateTime CreatedAt { get; set; }

    // 🔽 Yeni alanlar
    public bool IsTemporary { get; set; } = false;

    [JsonIgnore]
    public string FullName => $"{FirstName} {LastName}".Trim();
}
