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

            // **确保 UserId 类型匹配**
            if (!int.TryParse(userId, out int parsedUserId))
            {
                throw new ArgumentException("Invalid User ID format");
            }

            IQueryable<PersonalWord> query = _context.PersonalWords.Where(w => w.UserId == parsedUserId);

            switch (category.ToLower())
            {
                case "all":
                    // 返回所有单词
                    break;
                case "new":
                    // 可以添加新的分类逻辑，例如最近添加的单词
                    query = query.OrderByDescending(w => w.Id).Take(20);
                    break;
                default:
                    throw new ArgumentException("Invalid category");
            }

            return await query.ToListAsync();
        }
    }
}
