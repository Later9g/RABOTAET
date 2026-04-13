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
    public async Task<IActionResult> Get(
        Guid pageId,
        [FromQuery] bool? resolved,
        CancellationToken ct)
        => Ok(await _pageService.GetCommentsAsync(pageId, resolved, ct));

    [HttpPost]
    public async Task<IActionResult> Add(
        Guid pageId,
        [FromBody] AddCommentRequest request,
        CancellationToken ct)
        => Ok(await _pageService.AddCommentAsync(pageId, request, ct));

    [HttpPost("{commentId:guid}/replies")]
    public async Task<IActionResult> AddReply(
        Guid pageId,
        Guid commentId,
        [FromBody] AddCommentReplyRequest request,
        CancellationToken ct)
    {
        var result = await _pageService.AddCommentReplyAsync(pageId, commentId, request, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{commentId:guid}")]
    public async Task<IActionResult> UpdateRootComment(
        Guid pageId,
        Guid commentId,
        [FromBody] UpdateCommentRequest request,
        CancellationToken ct)
    {
        var result = await _pageService.UpdateCommentAsync(pageId, commentId, request, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{commentId:guid}/reply")]
    public async Task<IActionResult> UpdateReply(
        Guid pageId,
        Guid commentId,
        [FromBody] UpdateCommentRequest request,
        CancellationToken ct)
    {
        var result = await _pageService.UpdateCommentReplyAsync(pageId, commentId, request, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> Delete(
        Guid pageId,
        Guid commentId,
        CancellationToken ct)
    {
        var deleted = await _pageService.DeleteCommentAsync(pageId, commentId, ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{commentId:guid}/resolve")]
    public async Task<IActionResult> Resolve(
        Guid pageId,
        Guid commentId,
        CancellationToken ct)
    {
        var result = await _pageService.ResolveCommentAsync(pageId, commentId, true, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{commentId:guid}/reopen")]
    public async Task<IActionResult> Reopen(
        Guid pageId,
        Guid commentId,
        CancellationToken ct)
    {
        var result = await _pageService.ResolveCommentAsync(pageId, commentId, false, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{commentId:guid}/resolve")]
    public async Task<IActionResult> ResolvePatch(
        Guid pageId,
        Guid commentId,
        [FromBody] ResolveCommentRequest request,
        CancellationToken ct)
    {
        var result = await _pageService.ResolveCommentAsync(pageId, commentId, request.Resolved, ct);
        return result is null ? NotFound() : Ok(result);
    }
}