using Microsoft.AspNetCore.Identity;

namespace EnglishLearningAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = "/assets/avatar.png";   // 避免 CS8618 警告
    }
}
