using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnglishLearningAPI.Data;

[Route("api/recommendword")]
[ApiController]
public class RecommendWordController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly RecommendWordService _recommendWordService;

    public RecommendWordController(AppDbContext context, RecommendWordService recommendWordService)
    {
        _context = context;
        _recommendWordService = recommendWordService;
    }

    /// <summary>
    /// Get recommended words
    /// </summary>
    /// <param name="pastWords">The words the user has learned in the past, separated by commas</param>
    /// <param name="numWords">The number of words to recommend (default is 5)</param>
    /// <returns>Returns a list of recommended words</returns>
    [HttpGet("recommendations")]
    public async Task<IActionResult> GetRecommendedWords(
        [FromQuery] string? pastWords = "",  
        [FromQuery] int numWords = 5)  
    {
        if (string.IsNullOrWhiteSpace(pastWords))
        {
            // **Automatically populate the latest 3 words**
            var latestWords = await _context.PersonalWords
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => w.Word)
                .Take(3)  
                .ToListAsync();

            if (latestWords.Count == 0)
            {
                return BadRequest(new { error = "No words found in PersonalWords table!" });
            }

            pastWords = string.Join(",", latestWords);
        }

        var wordsArray = pastWords.Split(',');

        if (wordsArray.Length == 0)
        {
            return BadRequest(new { error = "No valid words found to generate recommendations!" });
        }

        try
        {
            // **🔹 Call Python API for predictions**
            var recommendations = await _recommendWordService.GetRecommendedWords(wordsArray, numWords);

            return Ok(new { status = "success", recommendedWords = recommendations });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to fetch recommended words", details = ex.Message });
        }
    }
}

