using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EnglishLearningAPI.Services;

namespace EnglishLearningAPI.Controllers
{
    [ApiController]
    [Route("api/dictionary")]
    public class DictionaryController : ControllerBase
    {
        [HttpGet("{word}")]
        public async Task<IActionResult> GetWordDefinition(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return BadRequest(new { error = "Word is required." });
            }

            string definition = await DictionaryService.GetWordDefinitionAsync(word);
            return Ok(new { word, definition });
        }
    }
}
