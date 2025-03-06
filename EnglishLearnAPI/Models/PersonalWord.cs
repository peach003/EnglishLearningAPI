using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningAPI.Models
{
    public class PersonalWord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!; // 关联用户的 ID

        [Required]
        public int WordId { get; set; } // 关联 Words 表的 ID

        [Required]
        public string Word { get; set; } = null!; // 单词内容

        [Required]
        public string Meaning { get; set; } = null!; // 单词释义

        public int FamiliarityLevel { get; set; } = 0; // 熟练度，默认 0

        public DateTime LastReviewed { get; set; } = DateTime.UtcNow; // 上次复习时间

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
