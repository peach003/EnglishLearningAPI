using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace EnglishLearningAPI.Controllers
{
    [Route("api/audio")]
    [ApiController]
    public class AudioController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _deepgramApiKey;

        public AudioController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _deepgramApiKey = configuration["Deepgram:ApiKey"] ?? throw new InvalidOperationException("Deepgram API Key is missing.");
        }

        [HttpPost("transcribe")]
        public async Task<IActionResult> TranscribeAudio(IFormFile audio)
        {
            if (audio == null || audio.Length == 0)
            {
                return BadRequest(new { error = "No audio file provided." });
            }

            using var memoryStream = new MemoryStream();
            await audio.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://api.deepgram.com/v1/listen?smart_format=true&punctuate=true&diarize=true&utterances=true&timestamps=true");

            request.Headers.Authorization = new AuthenticationHeaderValue("Token", _deepgramApiKey);
            request.Content = new StreamContent(memoryStream);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(audio.ContentType);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Deepgram API failed.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Deepgram API request failed", details = ex.Message });
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            DeepgramResponse? deepgramResult;
            try
            {
                deepgramResult = JsonSerializer.Deserialize<DeepgramResponse>(jsonResponse);
            }
            catch (JsonException)
            {
                return BadRequest(new { error = "Invalid Deepgram API response format." });
            }

            if (deepgramResult?.results?.channels == null ||
                deepgramResult.results.channels.Length == 0 ||
                deepgramResult.results.channels[0].alternatives == null ||
                deepgramResult.results.channels[0].alternatives.Length == 0)
            {
                return BadRequest(new { error = "Failed to process transcript." });
            }

            var alternative = deepgramResult.results.channels[0].alternatives[0];

            if (alternative.utterances != null && alternative.utterances.Length > 0)
            {
                var subtitles = alternative.utterances.Select(u => new SubtitleSegment
                {
                    Start = u.start,
                    End = u.end,
                    Text = u.transcript
                }).ToList();

                return Ok(new { subtitles });
            }

            var words = alternative.words;
            if (words == null || words.Length == 0)
            {
                return Ok(new { subtitles = new List<object>() });
            }

            var subtitlesList = new List<SubtitleSegment>();
            var currentSentence = new List<DeepgramWord>();
            float sentenceStartTime = words[0].start;

            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                currentSentence.Add(word);

                bool isEndOfSentence = word.word.EndsWith(".") || word.word.EndsWith("?") || word.word.EndsWith("!");
                bool isLongPause = (i > 0 && (word.start - words[i - 1].end) > 1.0f);

                if (isEndOfSentence || isLongPause)
                {
                    subtitlesList.Add(new SubtitleSegment
                    {
                        Start = sentenceStartTime,
                        End = word.end,
                        Text = string.Join(" ", currentSentence.Select(w => w.word))
                    });

                    currentSentence.Clear();

                    if (i < words.Length - 1)
                    {
                        sentenceStartTime = words[i + 1].start;
                    }
                }
            }

            if (currentSentence.Count > 0)
            {
                subtitlesList.Add(new SubtitleSegment
                {
                    Start = sentenceStartTime,
                    End = currentSentence.Last().end,
                    Text = string.Join(" ", currentSentence.Select(w => w.word))
                });
            }

            return Ok(new { subtitles = subtitlesList });
        }
    }

    public class DeepgramResponse
    {
        public DeepgramResults results { get; set; } = new DeepgramResults();
    }

    public class DeepgramResults
    {
        public DeepgramChannel[] channels { get; set; } = new DeepgramChannel[0];
    }

    public class DeepgramChannel
    {
        public DeepgramAlternative[] alternatives { get; set; } = new DeepgramAlternative[0];
    }

    public class DeepgramAlternative
    {
        public DeepgramWord[] words { get; set; } = new DeepgramWord[0];
        public DeepgramUtterance[] utterances { get; set; } = new DeepgramUtterance[0];
    }

    public class DeepgramWord
    {
        public string word { get; set; } = string.Empty;
        public float start { get; set; }
        public float end { get; set; }
    }

    public class DeepgramUtterance
    {
        public string transcript { get; set; } = string.Empty;
        public float start { get; set; }
        public float end { get; set; }
    }

    public class SubtitleSegment
    {
        public float Start { get; set; }
        public float End { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
