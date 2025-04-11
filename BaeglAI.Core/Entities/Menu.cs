public class Menu
{
    public string MenuId { get; set; }
    public string StoreId { get; set; }
    public List<MenuItem> Items { get; set; } = new();
}