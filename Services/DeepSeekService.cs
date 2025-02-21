using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EnglishLearningAPI.Services
{
    public class DeepSeekService
    {
        private const string ApiKey = "sk-35e7ced60bc94c1cb1c60d6756f07357";  // 🔹 替换为你的 DeepSeek API Key
        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> GenerateStudyTipAsync(string word)
        {
            string apiUrl = "https://api.deepseek.com/chat/completions";
            var requestData = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful AI assistant for language learning." },
                    new { role = "user", content = $"Provide a short memory tip for learning the word: {word}" }
                },
                stream = false
            };

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
            var requestBody = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, requestBody);
            string responseBody = await response.Content.ReadAsStringAsync();

            return JObject.Parse(responseBody)["choices"]?[0]?["message"]?["content"]?.ToString() ?? "No tip found.";
        }
    }
}
