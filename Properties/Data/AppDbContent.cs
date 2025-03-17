using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EnglishLearningAPI.Models;

namespace EnglishLearningAPI.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<PersonalWord> PersonalWords { get; set; } = null!;
        public DbSet<DiagramWord> DiagramWords { get; set; } 
        public DbSet<ClickLog> ClickLogs { get; set; }

       
    }

}



