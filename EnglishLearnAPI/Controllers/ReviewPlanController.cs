using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using EnglishLearningAPI.Services;
using EnglishLearningAPI.Models;

namespace EnglishLearningAPI.Controllers
{
    [ApiController]
    [Route("api/review-plan")]
    public class ReviewPlanController : ControllerBase
    {
        private readonly ReviewPlanService _reviewPlanService;

        public ReviewPlanController(ReviewPlanService reviewPlanService)
        {
            _reviewPlanService = reviewPlanService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePlan([FromQuery] string userId)
        {
            await _reviewPlanService.GenerateReviewPlanAsync(userId);
            return Ok(new { message = "Review plan generated successfully." });
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetUserPlans([FromQuery] string userId)
        {
            List<ReviewPlan> plans = await _reviewPlanService.GetUserReviewPlans(userId);
            return Ok(plans);
        }

        [HttpGet("study-tip")]
        public async Task<IActionResult> GetStudyTip([FromQuery] string word)
        {
            string tip = await DeepSeekService.GenerateStudyTipAsync(word);
            return Ok(new { word, tip });
        }
    }
}
