namespace EnglishLearningAPI.Models
{
    public class PersonalWord
    {
        public int Id { get; set; }
        public string? UserId { get; set; }  // 允许 null
        public string Word { get; set; } = string.Empty;  // 提供默认值
        public string Meaning { get; set; } = string.Empty; 
        public int Familiarity { get; set; } = 0; 
    }
}

