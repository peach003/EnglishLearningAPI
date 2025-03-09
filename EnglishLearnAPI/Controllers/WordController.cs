using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnglishLearningAPI.Data;  
using EnglishLearningAPI.Models; 

namespace EnglishLearningAPI.Controllers
{
    [Route("api/wordbook")]
    [ApiController]
    public class WordController : ControllerBase
    {
        private readonly AppDbContext _context;  

        public WordController(AppDbContext context)
        {
            _context = context;
        }

        // Add words to the PersonalWords table
        [HttpPost("add")]
        public async Task<IActionResult> AddWord([FromBody] PersonalWord wordEntry)
        {
            if (wordEntry == null || string.IsNullOrEmpty(wordEntry.Word))
            {
                return BadRequest(new { message = "Invalid word data." });
            }
            wordEntry.CreatedAt = DateTime.UtcNow;
            _context.PersonalWords.Add(wordEntry);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Word added successfully." });
        }

        // Get user's word list
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetWords(int userId)
        {
            var words = await _context.PersonalWords
                .Where(w => w.UserId == userId)
                .ToListAsync();

            return Ok(words);
        }

        [HttpGet("user/{userId}/words")]
        public async Task<IActionResult> GetUserWords(int userId)
        {
            var words = await _context.PersonalWords
                .Where(w => w.UserId == userId)
                .Select(w => new { w.Word }) 
                .ToListAsync();

            return Ok(words);
        }
    }
}
