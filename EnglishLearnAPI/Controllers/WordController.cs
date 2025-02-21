using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EnglishLearningAPI.Data;
using EnglishLearningAPI.Models;

namespace EnglishLearningAPI.Controllers
{
    [ApiController]
    [Route("api/words")]
    public class WordController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WordController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddWord([FromBody] WordModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Word) || string.IsNullOrEmpty(model.Meaning))
            {
                return BadRequest(new { error = "Word and Meaning are required." });
            }

            var word = new PersonalWord
            {
                UserId = model.UserId,
                Word = model.Word,
                Meaning = model.Meaning
            };

            _context.PersonalWords.Add(word);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Word added successfully!" });
        }
    }
}
