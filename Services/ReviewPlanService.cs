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

        // Get words from different wordbooks
        public async Task<List<PersonalWord>> GetWordsByCategoryAsync(string userId, string category)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty");
            }

            // Ensure that UserId type matches
            if (!int.TryParse(userId, out int parsedUserId))
            {
                throw new ArgumentException("Invalid User ID format");
            }

            IQueryable<PersonalWord> query = _context.PersonalWords.Where(w => w.UserId == parsedUserId);

            switch (category.ToLower())
            {
                case "all":
                    
                    break;
                case "new":
                    
                    query = query.OrderByDescending(w => w.Id).Take(20);
                    break;
                default:
                    throw new ArgumentException("Invalid category");
            }

            return await query.ToListAsync();
        }
    }
}
