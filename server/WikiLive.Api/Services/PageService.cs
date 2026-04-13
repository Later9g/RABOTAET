using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WikiLive.Api.Contracts;
using WikiLive.Api.Domain;
using WikiLive.Api.Hubs;
using WikiLive.Api.Infrastructure;

namespace WikiLive.Api.Services;

public interface IPageService
{
    Task<IReadOnlyList<PageSearchResult>> ListAsync(string? spaceId, string? search, CancellationToken ct);
    Task<Page> CreateAsync(CreatePageRequest request, CancellationToken ct);
    Task<Page?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<PageSearchResult>> SearchAsync(string query, CancellationToken ct);
    Task<(string Status, object Payload)> UpdateAsync(Guid id, UpdatePageRequest request, CancellationToken ct);
    Task<IReadOnlyList<object>> GetBacklinksAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<PageRevision>> GetRevisionsAsync(Guid pageId, CancellationToken ct);
    Task<object?> RestoreRevisionAsync(Guid pageId, Guid revisionId, CancellationToken ct);
    Task<IReadOnlyList<CommentThreadDto>> GetCommentsAsync(Guid pageId, bool? resolved, CancellationToken ct);
    Task<CommentThreadDto> AddCommentAsync(Guid pageId, AddCommentRequest request, CancellationToken ct);
    Task<CommentReplyDto?> AddCommentReplyAsync(Guid pageId, Guid commentId, AddCommentReplyRequest request, CancellationToken ct);
    Task<CommentThreadDto?> ResolveCommentAsync(Guid pageId, Guid commentId, bool resolved, CancellationToken ct);
    Task<CommentThreadDto?> UpdateCommentAsync(Guid pageId, Guid commentId, UpdateCommentRequest request, CancellationToken ct);
    Task<CommentReplyDto?> UpdateCommentReplyAsync(Guid pageId, Guid commentId, UpdateCommentRequest request, CancellationToken ct);
    Task<bool> DeleteCommentAsync(Guid pageId, Guid commentId, CancellationToken ct);
}

public sealed class PageService : IPageService
{
    private readonly WikiDbContext _db;
    private readonly ILinkParserService _linkParser;
    private readonly IUserContext _userContext;
    private readonly IHubContext<PageHub> _hub;

    public PageService(
        WikiDbContext db,
        ILinkParserService linkParser,
        IUserContext userContext,
        IHubContext<PageHub> hub)
    {
        _db = db;
        _linkParser = linkParser;
        _userContext = userContext;
        _hub = hub;
    }

    public async Task<CommentThreadDto?> UpdateCommentAsync(
    Guid pageId,
    Guid commentId,
    UpdateCommentRequest request,
    CancellationToken ct)
    {
        var comment = await _db.PageComments
            .FirstOrDefaultAsync(x => x.Id == commentId && x.PageId == pageId, ct);

        if (comment == null)
            return null;

        if (comment.ParentCommentId != null)
            return null;

        comment.Text = request.Text;
        await _db.SaveChangesAsync(ct);

        var replies = await _db.PageComments
            .AsNoTracking()
            .Where(x => x.ParentCommentId == comment.Id)
            .OrderBy(x => x.CreatedAtUtc)
            .Select(x => new CommentReplyDto(
                x.Id,
                x.PageId,
                x.ParentCommentId!.Value,
                x.Text,
                x.AuthorId,
                x.CreatedAtUtc))
            .ToListAsync(ct);

        var dto = new CommentThreadDto(
            comment.Id,
            comment.PageId,
            comment.AnchorJson,
            comment.Text,
            comment.AuthorId,
            comment.CreatedAtUtc,
            comment.IsResolved,
            comment.ResolvedBy,
            comment.ResolvedAtUtc,
            replies);

        await _hub.Clients.Group($"page:{pageId}")
            .SendAsync("CommentUpdated", dto, ct);

        return dto;
    }

    public async Task<CommentReplyDto?> UpdateCommentReplyAsync(
        Guid pageId,
        Guid commentId,
        UpdateCommentRequest request,
        CancellationToken ct)
    {
        var reply = await _db.PageComments
            .FirstOrDefaultAsync(x => x.Id == commentId && x.PageId == pageId, ct);

        if (reply == null)
            return null;

        if (reply.ParentCommentId == null)
            return null;

        reply.Text = request.Text;
        await _db.SaveChangesAsync(ct);

        return new CommentReplyDto(
            reply.Id,
            reply.PageId,
            reply.ParentCommentId.Value,
            reply.Text,
            reply.AuthorId,
            reply.CreatedAtUtc);

    }

    public async Task<bool> DeleteCommentAsync(Guid pageId, Guid commentId, CancellationToken ct)
    {
        var comment = await _db.PageComments
            .FirstOrDefaultAsync(x => x.Id == commentId && x.PageId == pageId, ct);

        if (comment == null)
            return false;

        if (comment.ParentCommentId == null)
        {
            // root comment -> удалить весь thread
            var replies = await _db.PageComments
                .Where(x => x.ParentCommentId == comment.Id)
                .ToListAsync(ct);

            _db.PageComments.RemoveRange(replies);
            _db.PageComments.Remove(comment);
        }
        else
        {
            // reply -> удалить только reply
            _db.PageComments.Remove(comment);
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }
    public async Task<IReadOnlyList<CommentThreadDto>> GetCommentsAsync(Guid pageId, bool? resolved, CancellationToken ct)
    {
        var comments = await _db.PageComments
            .AsNoTracking()
            .Where(x => x.PageId == pageId)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(ct);

        var roots = comments
            .Where(x => x.ParentCommentId == null)
            .ToList();

        if (resolved.HasValue)
            roots = roots.Where(x => x.IsResolved == resolved.Value).ToList();

        var repliesLookup = comments
            .Where(x => x.ParentCommentId != null)
            .GroupBy(x => x.ParentCommentId!.Value)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<CommentReplyDto>)g
                    .OrderBy(x => x.CreatedAtUtc)
                    .Select(x => new CommentReplyDto(
                        x.Id,
                        x.PageId,
                        x.ParentCommentId!.Value,
                        x.Text,
                        x.AuthorId,
                        x.CreatedAtUtc))
                    .ToList());

        var result = roots
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(root => new CommentThreadDto(
                root.Id,
                root.PageId,
                root.AnchorJson,
                root.Text,
                root.AuthorId,
                root.CreatedAtUtc,
                root.IsResolved,
                root.ResolvedBy,
                root.ResolvedAtUtc,
                repliesLookup.TryGetValue(root.Id, out var replies)
                    ? replies
                    : Array.Empty<CommentReplyDto>()))
            .ToList();

        return result;
    }

    public async Task<CommentThreadDto> AddCommentAsync(Guid pageId, AddCommentRequest request, CancellationToken ct)
    {
        var pageExists = await _db.Pages.AnyAsync(x => x.Id == pageId, ct);
        if (!pageExists)
            throw new Exception("Page not found");

        var comment = new PageComment
        {
            Id = Guid.NewGuid(),
            PageId = pageId,
            ParentCommentId = null,
            AnchorJson = request.AnchorJson,
            Text = request.Text,
            AuthorId = _userContext.GetUserId(),
            CreatedAtUtc = DateTime.UtcNow,
            IsResolved = false,
            ResolvedBy = null,
            ResolvedAtUtc = null
        };

        _db.PageComments.Add(comment);
        await _db.SaveChangesAsync(ct);

        var dto = new CommentThreadDto(
            comment.Id,
            comment.PageId,
            comment.AnchorJson,
            comment.Text,
            comment.AuthorId,
            comment.CreatedAtUtc,
            comment.IsResolved,
            comment.ResolvedBy,
            comment.ResolvedAtUtc,
            Array.Empty<CommentReplyDto>());

        await _hub.Clients.Group($"page:{pageId}")
            .SendAsync("CommentAdded", dto, ct);

        return dto;
    }

    public async Task<CommentReplyDto?> AddCommentReplyAsync(
    Guid pageId,
    Guid commentId,
    AddCommentReplyRequest request,
    CancellationToken ct)
    {
        var root = await _db.PageComments
            .FirstOrDefaultAsync(x => x.Id == commentId && x.PageId == pageId, ct);

        if (root == null)
            return null;

        if (root.ParentCommentId != null)
            throw new Exception("Replies can only be added to root comments");

        var reply = new PageComment
        {
            Id = Guid.NewGuid(),
            PageId = pageId,
            ParentCommentId = root.Id,
            AnchorJson = "{}",
            Text = request.Text,
            AuthorId = _userContext.GetUserId(),
            CreatedAtUtc = DateTime.UtcNow,
            IsResolved = false,
            ResolvedBy = null,
            ResolvedAtUtc = null
        };

        _db.PageComments.Add(reply);
        await _db.SaveChangesAsync(ct);

        var dto = new CommentReplyDto(
            reply.Id,
            reply.PageId,
            reply.ParentCommentId!.Value,
            reply.Text,
            reply.AuthorId,
            reply.CreatedAtUtc);

        await _hub.Clients.Group($"page:{pageId}")
            .SendAsync("CommentReplyAdded", dto, ct);

        return dto;
    }

    public async Task<CommentThreadDto?> ResolveCommentAsync(Guid pageId, Guid commentId, bool resolved, CancellationToken ct)
    {
        var comment = await _db.PageComments
            .FirstOrDefaultAsync(x => x.Id == commentId && x.PageId == pageId, ct);

        if (comment == null)
            return null;

        if (comment.ParentCommentId != null)
            throw new Exception("Only root comments can be resolved");

        comment.IsResolved = resolved;

        if (resolved)
        {
            comment.ResolvedBy = _userContext.GetUserId();
            comment.ResolvedAtUtc = DateTime.UtcNow;
        }
        else
        {
            comment.ResolvedBy = null;
            comment.ResolvedAtUtc = null;
        }

        await _db.SaveChangesAsync(ct);

        var replies = await _db.PageComments
            .AsNoTracking()
            .Where(x => x.ParentCommentId == comment.Id)
            .OrderBy(x => x.CreatedAtUtc)
            .Select(x => new CommentReplyDto(
                x.Id,
                x.PageId,
                x.ParentCommentId!.Value,
                x.Text,
                x.AuthorId,
                x.CreatedAtUtc))
            .ToListAsync(ct);

        var dto = new CommentThreadDto(
            comment.Id,
            comment.PageId,
            comment.AnchorJson,
            comment.Text,
            comment.AuthorId,
            comment.CreatedAtUtc,
            comment.IsResolved,
            comment.ResolvedBy,
            comment.ResolvedAtUtc,
            replies);

        await _hub.Clients.Group($"page:{pageId}")
            .SendAsync("CommentResolved", dto, ct);

        return dto;
    }

    public async Task<IReadOnlyList<PageSearchResult>> ListAsync(string? spaceId, string? search, CancellationToken ct)
    {
        var query = _db.Pages.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(spaceId))
            query = query.Where(x => x.SpaceId == spaceId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Title.ToLower().Contains(search.ToLower()));

        return await query
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Select(x => new PageSearchResult(x.Id, x.Title, x.Slug, x.UpdatedAtUtc))
            .ToListAsync(ct);
    }

    public async Task<Page> CreateAsync(CreatePageRequest request, CancellationToken ct)
    {
        var userId = _userContext.GetUserId();

        var page = new Page
        {
            Id = Guid.NewGuid(),
            SpaceId = request.SpaceId,
            Title = request.Title,
            Slug = Slugify(request.Title),
            ContentJson = "{\"type\":\"doc\",\"content\":[{\"type\":\"paragraph\"}]}",
            Version = 1,
            CreatedBy = userId,
            UpdatedBy = userId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _db.Pages.Add(page);
        _db.PageRevisions.Add(new PageRevision
        {
            Id = Guid.NewGuid(),
            PageId = page.Id,
            Title = page.Title,
            ContentJson = page.ContentJson,
            Version = 1,
            Source = "create",
            CreatedBy = userId,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return page;
    }

    public Task<Page?> GetAsync(Guid id, CancellationToken ct)
        => _db.Pages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)!;

    public async Task<IReadOnlyList<PageSearchResult>> SearchAsync(string query, CancellationToken ct)
    {
        query ??= string.Empty;

        return await _db.Pages.AsNoTracking()
            .Where(x => x.Title.ToLower().Contains(query.ToLower()))
            .OrderBy(x => x.Title)
            .Select(x => new PageSearchResult(x.Id, x.Title, x.Slug, x.UpdatedAtUtc))
            .Take(20)
            .ToListAsync(ct);
    }

    public async Task<(string Status, object Payload)> UpdateAsync(Guid id, UpdatePageRequest request, CancellationToken ct)
    {
        var page = await _db.Pages.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (page is null)
            return ("not_found", new { message = "Page not found" });

        if (page.Version != request.Version)
        {
            return ("conflict", new
            {
                message = "Version conflict",
                serverVersion = page.Version,
                serverContentJson = page.ContentJson,
                serverUpdatedAtUtc = page.UpdatedAtUtc
            });
        }

        var userId = _userContext.GetUserId();

        page.Title = request.Title;
        page.Slug = Slugify(request.Title);
        page.ContentJson = request.ContentJson;
        page.Version += 1;
        page.UpdatedBy = userId;
        page.UpdatedAtUtc = DateTime.UtcNow;

        if (request.CreateRevision || page.Version % 5 == 0)
        {
            _db.PageRevisions.Add(new PageRevision
            {
                Id = Guid.NewGuid(),
                PageId = page.Id,
                Title = page.Title,
                ContentJson = page.ContentJson,
                Version = page.Version,
                Source = request.CreateRevision ? "manual" : "autosave",
                CreatedBy = userId,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        RebuildLinks(page);

        await _db.SaveChangesAsync(ct);

        await _hub.Clients.Group($"page:{page.Id}")
            .SendAsync("PageSaved", new { page.Id, page.Version }, ct);

        return ("ok", new
        {
            page.Id,
            page.Title,
            page.ContentJson,
            page.Version,
            page.UpdatedAtUtc
        });
    }

    public async Task<IReadOnlyList<object>> GetBacklinksAsync(Guid id, CancellationToken ct)
    {
        return await (
            from link in _db.PageLinks.AsNoTracking()
            join page in _db.Pages.AsNoTracking() on link.FromPageId equals page.Id
            where link.ToPageId == id
            select (object)new
            {
                page.Id,
                page.Title,
                page.Slug,
                page.UpdatedAtUtc
            })
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PageRevision>> GetRevisionsAsync(Guid pageId, CancellationToken ct)
        => await _db.PageRevisions.AsNoTracking()
            .Where(x => x.PageId == pageId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task<object?> RestoreRevisionAsync(Guid pageId, Guid revisionId, CancellationToken ct)
    {
        var page = await _db.Pages.FirstOrDefaultAsync(x => x.Id == pageId, ct);
        var revision = await _db.PageRevisions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == revisionId && x.PageId == pageId, ct);

        if (page is null || revision is null)
            return null;

        var userId = _userContext.GetUserId();

        page.Title = revision.Title;
        page.Slug = Slugify(revision.Title);
        page.ContentJson = revision.ContentJson;
        page.Version += 1;
        page.UpdatedBy = userId;
        page.UpdatedAtUtc = DateTime.UtcNow;

        _db.PageRevisions.Add(new PageRevision
        {
            Id = Guid.NewGuid(),
            PageId = page.Id,
            Title = page.Title,
            ContentJson = page.ContentJson,
            Version = page.Version,
            Source = "restore",
            CreatedBy = userId,
            CreatedAtUtc = DateTime.UtcNow
        });

        RebuildLinks(page);

        await _db.SaveChangesAsync(ct);

        return new
        {
            page.Id,
            page.Title,
            page.ContentJson,
            page.Version,
            page.UpdatedAtUtc,
            restoredFromRevisionId = revisionId
        };
    }

    public async Task<IReadOnlyList<PageComment>> GetCommentsAsync(Guid pageId, CancellationToken ct)
    {
        return await _db.PageComments
            .AsNoTracking()
            .Where(x => x.PageId == pageId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);
    }

    private void RebuildLinks(Page page)
    {
        var oldLinks = _db.PageLinks.Where(x => x.FromPageId == page.Id);
        _db.PageLinks.RemoveRange(oldLinks);

        var parsedLinks = _linkParser.Parse(page.ContentJson);

        foreach (var link in parsedLinks.DistinctBy(x => x.PageId))
        {
            _db.PageLinks.Add(new PageLink
            {
                Id = Guid.NewGuid(),
                FromPageId = page.Id,
                ToPageId = link.PageId,
                LinkText = link.Title,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
    }

    private static string Slugify(string title)
        => title.Trim().ToLowerInvariant().Replace(' ', '-');
}