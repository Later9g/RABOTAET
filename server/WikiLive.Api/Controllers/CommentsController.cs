using Microsoft.AspNetCore.Mvc;
using WikiLive.Api.Contracts;
using WikiLive.Api.Services;

namespace WikiLive.Api.Controllers;

[ApiController]
[Route("api/pages/{pageId:guid}/comments")]
public class CommentsController : ControllerBase
{
    private readonly IPageService _pageService;

    public CommentsController(IPageService pageService)
    {
        _pageService = pageService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid pageId, CancellationToken ct)
        => Ok(await _pageService.GetCommentsAsync(pageId, ct));

    [HttpPost]
    public async Task<IActionResult> Add(Guid pageId, [FromBody] AddCommentRequest request, CancellationToken ct)
        => Ok(await _pageService.AddCommentAsync(pageId, request, ct));
}
