using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class RecommendWordService
{
    private readonly HttpClient _httpClient;

    public RecommendWordService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Get recommended words via FastAPI
    public async Task<List<string>> GetRecommendedWords(string[] pastWords, int numWords)
    {
        string wordList = string.Join(",", pastWords);
        string url = $"http://localhost:5001/predict?words={wordList}&num_predictions={numWords}";

        var response = await _httpClient.GetStringAsync(url);
        var result = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(response);

        return result["predicted_words"];
    }
}
