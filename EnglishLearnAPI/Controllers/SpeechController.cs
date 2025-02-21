using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using EnglishLearningAPI.Services;

namespace EnglishLearningAPI.Controllers
{
    [ApiController]
    [Route("api/speech")]
    public class SpeechController : ControllerBase
    {
        [HttpPost("transcribe")]
        public async Task<IActionResult> TranscribeAudio([FromBody] SpeechRequest request)
        {
            if (string.IsNullOrEmpty(request.FilePath))
            {
                return BadRequest(new { error = "FilePath is required." });
            }

            JArray subtitles = await DeepgramSpeechService.RecognizeSpeechWithTimestampsAsync(request.FilePath);

            return Ok(new { subtitles });
        }
    }

    public class SpeechRequest
    {
        public string? FilePath { get; set; } // 允许 null，防止 `CS8618`
    }
}

