using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnglishLearningAPI.Data;  // ⚠️ 确保正确引用
using EnglishLearningAPI.Models; // ⚠️ 引用 PersonalWord 模型

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

        // 添加单词到 PersonalWords 表
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

        // 获取用户的单词列表
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
                .Select(w => new { w.Word }) // 仅返回单词
                .ToListAsync();

            return Ok(words);
        }
    }
}
