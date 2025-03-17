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
    /// 获取最近 10 天 `DiagramWord` 新增数量
    /// </summary>
    [HttpGet("stats/last10days")]
    public async Task<IActionResult> GetLast10DaysStats()
    {
        var stats = await _diagramWordService.GetLast10DaysStatsAsync();
        return Ok(stats);
    }
}
