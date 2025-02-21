using System;
using System.IO;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace EnglishLearningAPI.Services
{
    public class DeepgramSpeechService
    {
        private const string ApiKey = "YOUR_DEEPGRAM_API_KEY"; // 🔹 你的 Deepgram API Key

        public static async Task<JArray> RecognizeSpeechWithTimestampsAsync(string filePath)
        {
            var client = new RestClient("https://api.deepgram.com/v1/listen?punctuate=true&diarize=true&utterances=true");
            var request = new RestRequest()
                .AddHeader("Authorization", $"Token {ApiKey}")
                .AddHeader("Content-Type", "audio/wav");

            request.AddFile("audio", filePath);
            var response = await client.ExecutePostAsync(request);

            if (string.IsNullOrEmpty(response.Content))
            {
                return new JArray { new JObject { ["error"] = "No response from Deepgram API." } };
            }

            var json = JObject.Parse(response.Content);
            return json["results"]?["utterances"] as JArray ?? new JArray();
        }
    }
}

