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
        [HttpPost("click")]
public async Task<IActionResult> RecordClick([FromBody] ClickLog request)
{
    var wordEntry = await _context.PersonalWords
        .FirstOrDefaultAsync(w => w.UserId == request.UserId && w.Word == request.Word);

    if (wordEntry == null)
    {
        return NotFound("Word not found for this user.");
    }

    // **记录点击**
    var clickLog = new ClickLog
    {
        UserId = request.UserId,
        Word = request.Word
    };
    _context.ClickLogs.Add(clickLog);

    // **增加熟悉度**
    wordEntry.Familiarity += 1;

    await _context.SaveChangesAsync();
    return Ok(new { message = "Click recorded and Familiarity updated!", Familiarity = wordEntry.Familiarity });
}

    }
}
