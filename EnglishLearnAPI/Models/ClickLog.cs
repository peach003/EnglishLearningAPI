public class ClickLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Word { get; set; } = string.Empty;
    public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
}
