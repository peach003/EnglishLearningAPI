using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EnglishLearningAPI.Services
{
    public class DictionaryService
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> GetWordDefinitionAsync(string word)
        {
            string apiUrl = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                JArray json = JArray.Parse(responseBody);

                return json[0]?["meanings"]?[0]?["definitions"]?[0]?["definition"]?.ToString() ?? "No definition found.";
            }
            catch
            {
                return "Error retrieving definition."; //  防止 `CS8604` 崩溃
            }
        }
    }
}
