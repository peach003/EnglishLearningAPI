using EnglishLearningAPI.Data;
using EnglishLearningAPI.Models;  // ✅ 确保正确引用
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

        public async Task GenerateReviewPlanAsync(string userId)
        {
            var words = await _context.PersonalWords
                .Where(w => w.UserId == userId)
                .ToListAsync();

            var today = DateTime.UtcNow;

            foreach (var word in words)
            {
                int[] reviewIntervals;

                // 根据熟练度动态调整复习间隔
                if (word.Familiarity == 0)
                    reviewIntervals = new int[] { 0, 1, 3, 7, 14 }; // 新学单词
                else if (word.Familiarity >= 3)
                    reviewIntervals = new int[] { 0, 2, 5, 10, 21 }; // 熟悉单词
                else
                    reviewIntervals = new int[] { 0, 3, 7, 14, 28 }; // 半熟练单词

                foreach (var interval in reviewIntervals)
                {
                    var reviewDate = today.AddDays(interval);

                    var existingPlan = await _context.ReviewPlans
                        .FirstOrDefaultAsync(r => r.UserId == userId && r.WordId == word.Id && r.ReviewDate == reviewDate);
                    
                    if (existingPlan == null)
                    {
                        _context.ReviewPlans.Add(new ReviewPlan
                        {
                            UserId = userId,
                            WordId = word.Id,
                            ReviewDate = reviewDate,
                            Completed = false
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<ReviewPlan>> GetUserReviewPlans(string userId)
        {
            return await _context.ReviewPlans
                .Where(r => r.UserId == userId && r.ReviewDate >= DateTime.UtcNow && !r.Completed)
                .OrderBy(r => r.ReviewDate)
                .ToListAsync();
        }
    }
}
