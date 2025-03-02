using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using EnglishLearningAPI.Services;

[ApiController]
[Route("api/speech")]
public class SpeechController : ControllerBase
{
    private readonly string _uploadPath = "Uploads";

    /// <summary>
    /// ✅ 上传音频文件并获取完整字幕
    /// </summary>
    [HttpPost("upload-and-transcribe")]
    public async Task<IActionResult> UploadAndTranscribe([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded." });

        try
        {
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }

            var filePath = Path.Combine(_uploadPath, Path.GetRandomFileName() + Path.GetExtension(file.FileName));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // ✅ 调用 Deepgram 进行语音识别
            JArray subtitles = await DeepgramSpeechService.RecognizeSpeechWithTimestampsAsync(filePath);

            return Ok(new { subtitles });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Server error: {ex.Message}" });
        }
    }

    /// <summary>
    /// ✅ WebSocket 端点，支持实时语音流字幕生成
    /// </summary>
    [HttpGet("stream")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using WebSocket clientSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await DeepgramSpeechService.StreamToDeepgram(clientSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }
    }
}
