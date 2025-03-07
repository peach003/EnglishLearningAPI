public class PersonalWord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Word { get; set; } = string.Empty; 
    public int Familiarity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
