namespace EnglishLearningAPI.Models
{
    public class ReviewPlan
    {
        public int Id { get; set; }
        public string? UserId { get; set; }  // 允许 null
        public int WordId { get; set; }
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;  //  提供默认值
        public bool Completed { get; set; } = false;  // 提供默认值
    }
}
