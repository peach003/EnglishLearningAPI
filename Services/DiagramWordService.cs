using Microsoft.EntityFrameworkCore;
using EnglishLearningAPI.Data;  
using EnglishLearningAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DiagramWordService
{
    private readonly AppDbContext _context;

    public DiagramWordService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取最近 10 天每天新增的 DiagramWord 数量
    /// </summary>
    public async Task<List<DiagramWordStatsDto>> GetLast10DaysStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-9);  // 10 天前的日期（包含今天）

        // 获取 10 天内的数据并按日期统计
        var wordCounts = await _context.DiagramWords
            .Where(w => w.CreatedAt >= startDate)  // 过滤 10 天内的数据
            .GroupBy(w => w.CreatedAt.Date)        // 按日期分组
            .Select(g => new 
            {
                Date = g.Key,
                WordCount = g.Count()
            })
            .ToDictionaryAsync(x => x.Date.ToString("yyyy-MM-dd"), x => x.WordCount); 

        // 构建完整的 10 天数据，确保每一天都有数据（即使是 0）
        return Enumerable.Range(0, 10)
            .Select(i =>
            {
                var dateStr = startDate.AddDays(i).ToString("yyyy-MM-dd");
                return new DiagramWordStatsDto
                {
                    Date = dateStr,
                    WordCount = wordCounts.ContainsKey(dateStr) ? wordCounts[dateStr] : 0
                };
            })
            .ToList();
    }
}

public class DiagramWordStatsDto
{
    public string Date { get; set; }
    public int WordCount { get; set; }
}
