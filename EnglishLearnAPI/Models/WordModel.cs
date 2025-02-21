namespace EnglishLearningAPI.Models
{
    public class WordModel
    {
        public string? UserId { get; set; }  // 允许 null
        public string Word { get; set; } = string.Empty;  // 提供默认值
        public string Meaning { get; set; } = string.Empty;  // 提供默认值
    }
}

