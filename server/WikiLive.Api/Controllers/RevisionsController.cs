using Microsoft.AspNetCore.Mvc;
using WikiLive.Api.Services;

namespace WikiLive.Api.Controllers;

[ApiController]
[Route("api/pages/{pageId:guid}/revisions")]
public class RevisionsController : ControllerBase
{
    private readonly IPageService _pageService;

    public RevisionsController(IPageService pageService)
    {
        _pageService = pageService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid pageId, CancellationToken ct)
        => Ok(await _pageService.GetRevisionsAsync(pageId, ct));

    [HttpPost("{revisionId:guid}/restore")]
    public async Task<IActionResult> Restore(Guid pageId, Guid revisionId, CancellationToken ct)
    {
        var restored = await _pageService.RestoreRevisionAsync(pageId, revisionId, ct);
        return restored is null ? NotFound() : Ok(restored);
    }
}
