using Microsoft.AspNetCore.Mvc;
using WikiLive.Api.Contracts;
using WikiLive.Api.Services;

namespace WikiLive.Api.Controllers;

[ApiController]
[Route("api/pages")]
public class PagesController : ControllerBase
{
    private readonly IPageService _pageService;

    public PagesController(IPageService pageService)
    {
        _pageService = pageService;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? spaceId, [FromQuery] string? search, CancellationToken ct)
        => Ok(await _pageService.ListAsync(spaceId, search, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePageRequest request, CancellationToken ct)
        => Ok(await _pageService.CreateAsync(request, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var page = await _pageService.GetAsync(id, ct);
        return page is null ? NotFound() : Ok(page);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, CancellationToken ct)
        => Ok(await _pageService.SearchAsync(query, ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePageRequest request, CancellationToken ct)
    {
        var result = await _pageService.UpdateAsync(id, request, ct);

        return result.Status switch
        {
            "ok" => Ok(result.Payload),
            "conflict" => Conflict(result.Payload),
            _ => NotFound(result.Payload)
        };
    }

    [HttpGet("{id:guid}/backlinks")]
    public async Task<IActionResult> Backlinks(Guid id, CancellationToken ct)
        => Ok(await _pageService.GetBacklinksAsync(id, ct));
}