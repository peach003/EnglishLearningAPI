using EnglishLearningAPI.Data;
using EnglishLearningAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnglishLearningAPI.Services
{
    public class ReviewPlanService
    {
        private readonly AppDbContext _context;

        public ReviewPlanService(AppDbContext context)
        {
            _context = context;
        }

        // 获取不同单词本的单词
        public async Task<List<PersonalWord>> GetWordsByCategoryAsync(string userId, string category)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty");
            }

            // **确保 UserId 是 string 类型，避免 int 转换错误**
            IQueryable<PersonalWord> query = _context.PersonalWords.Where(w => w.UserId == userId);

            var today = DateTime.UtcNow;
            DateTime newWordsThreshold = today.AddDays(-14);
            DateTime beginnerThreshold = today.AddDays(-28);
            DateTime intermediateThreshold = today.AddDays(-60);
            DateTime advancedThreshold = today.AddDays(-180);

            switch (category.ToLower())
            {
                case "new":
                    query = query.Where(w => w.LastReviewed >= newWordsThreshold);
                    break;
                case "beginner":
                    query = query.Where(w => w.LastReviewed >= beginnerThreshold);
                    break;
                case "intermediate":
                    query = query.Where(w => w.LastReviewed >= intermediateThreshold);
                    break;
                case "advanced":
                    query = query.Where(w => w.LastReviewed >= advancedThreshold);
                    break;
                case "recommended":
                    query = ApplySmartRecommendation(query);
                    break;
                default:
                    throw new ArgumentException("Invalid category");
            }

            return await query.ToListAsync();
        }

        // **推荐单词本 (智能算法)**
        private IQueryable<PersonalWord> ApplySmartRecommendation(IQueryable<PersonalWord> query)
        {
            return query.OrderBy(w => w.FamiliarityLevel).ThenBy(w => w.LastReviewed);
        }
    }
}
