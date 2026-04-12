using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WikiLive.Api.Infrastructure;

namespace WikiLive.Api.Controllers;

[ApiController]
[Route("api/graph")]
public class GraphController : ControllerBase
{
    private readonly WikiDbContext _db;

    public GraphController(WikiDbContext db)
    {
        _db = db;
    }

    [HttpGet("space/{spaceId}")]
    public async Task<IActionResult> GetGraph(string spaceId, CancellationToken ct)
    {
        var nodes = await _db.Pages.Where(x => x.SpaceId == spaceId).Select(x => new { x.Id, x.Title }).ToListAsync(ct);
        var ids = nodes.Select(x => x.Id).ToHashSet();
        var edges = await _db.PageLinks.Where(x => ids.Contains(x.FromPageId) && ids.Contains(x.ToPageId))
            .Select(x => new { x.FromPageId, x.ToPageId })
            .ToListAsync(ct);
        return Ok(new { nodes, edges });
    }
}
