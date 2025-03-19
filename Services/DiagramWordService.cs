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
    /// Get the number of DiagramWords added each day in the last 10 days
    /// </summary>
    public async Task<List<DiagramWordStatsDto>> GetLast10DaysStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-9);  // Date 10 days ago (including today)

        // Get data for the last 10 days and group by date
        var wordCounts = await _context.DiagramWords
            .Where(w => w.CreatedAt >= startDate)  // Filter data from the last 10 days
            .GroupBy(w => w.CreatedAt.Date)        // Group by date
            .Select(g => new 
            {
                Date = g.Key,
                WordCount = g.Count()
            })
            .ToDictionaryAsync(x => x.Date.ToString("yyyy-MM-dd"), x => x.WordCount); 

        // Build the complete 10 days data, ensuring every day has data (even if it's 0)
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
