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
    /// 获取推荐单词
    /// </summary>
    /// <param name="pastWords">用户过去学习过的单词，逗号分隔</param>
    /// <param name="numWords">需要推荐的单词数量（默认5个）</param>
    /// <returns>返回推荐的单词列表</returns>
    [HttpGet("recommendations")]
    public async Task<IActionResult> GetRecommendedWords(
        [FromQuery] string? pastWords = "",  
        [FromQuery] int numWords = 5)  
    {
        if (string.IsNullOrWhiteSpace(pastWords))
        {
            // **自动填充最近的 3 个单词**
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
            // **🔹 调用 Python API 进行预测**
            var recommendations = await _recommendWordService.GetRecommendedWords(wordsArray, numWords);

            return Ok(new { status = "success", recommendedWords = recommendations });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to fetch recommended words", details = ex.Message });
        }
    }
}

