using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnglishLearningAPI.Models;
using EnglishLearningAPI.Data;

namespace EnglishLearningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewPlanController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewPlanController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetReviewPlan(int userId)
        {
            var today = DateTime.UtcNow;
            DateTime within3Days = today.AddDays(-3);
            DateTime within7Days = today.AddDays(-7);
            DateTime within14Days = today.AddDays(-14);
            DateTime within28Days = today.AddDays(-28);

            var wordsWithin3Days = await _context.PersonalWords
                .Where(w => w.UserId == userId && w.CreatedAt > within3Days)
                .ToListAsync();

            var wordsWithin7Days = await _context.PersonalWords
                .Where(w => w.UserId == userId && w.CreatedAt <= within3Days && w.CreatedAt > within7Days)
                .ToListAsync();

            var wordsWithin14Days = await _context.PersonalWords
                .Where(w => w.UserId == userId && w.CreatedAt <= within7Days && w.CreatedAt > within14Days)
                .ToListAsync();

            var wordsWithin28Days = await _context.PersonalWords
                .Where(w => w.UserId == userId && w.CreatedAt <= within14Days && w.CreatedAt > within28Days)
                .ToListAsync();

            var result = new Dictionary<string, List<PersonalWord>>
            {
                { "3Days", wordsWithin3Days },
                { "7Days", wordsWithin7Days },
                { "14Days", wordsWithin14Days },
                { "28Days", wordsWithin28Days }
            };

            return Ok(result);
        }
    }
}
