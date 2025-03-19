using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/diagramword")]
[ApiController]
public class DiagramWordController : ControllerBase
{
    private readonly DiagramWordService _diagramWordService;

    public DiagramWordController(DiagramWordService diagramWordService)
    {
        _diagramWordService = diagramWordService;
    }

    /// <summary>
    /// Get the number of new DiagramWord entries in the last 10 days.
    /// </summary>
    [HttpGet("stats/last10days")]
    public async Task<IActionResult> GetLast10DaysStats()
    {
        var stats = await _diagramWordService.GetLast10DaysStatsAsync();
        return Ok(stats);
    }
}
