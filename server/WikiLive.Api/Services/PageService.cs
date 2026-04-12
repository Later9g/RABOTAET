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
    Task<PageComment> AddCommentAsync(Guid pageId, AddCommentRequest request, CancellationToken ct);
    Task<IReadOnlyList<PageComment>> GetCommentsAsync(Guid pageId, CancellationToken ct);
}

public class PageService : IPageService
{
    private readonly WikiDbContext _db;
    private readonly ILinkParserService _linkParser;
    private readonly IHubContext<PageHub> _hub;
    private readonly IUserContext _userContext;

    public PageService(WikiDbContext db, ILinkParserService linkParser, IHubContext<PageHub> hub, IUserContext userContext)
    {
        _db = db;
        _linkParser = linkParser;
        _hub = hub;
        _userContext = userContext;
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
            Version = page.Version,
            CreatedBy = userId,
            Source = "create",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return page;
    }

    public Task<Page?> GetAsync(Guid id, CancellationToken ct) =>
        _db.Pages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)!;

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
        if (page is null) return ("not_found", new { message = "Not found" });

        if (page.Version != request.Version)
        {
            return ("conflict", new { message = "Version conflict", serverVersion = page.Version, serverContentJson = page.ContentJson, serverUpdatedAtUtc = page.UpdatedAtUtc });
        }

        var userId = _userContext.GetUserId();
        page.Title = request.Title;
        page.Slug = Slugify(request.Title);
        page.ContentJson = request.ContentJson;
        page.Version += 1;
        page.UpdatedAtUtc = DateTime.UtcNow;
        page.UpdatedBy = userId;

        if (request.CreateRevision || page.Version % 5 == 0)
        {
            _db.PageRevisions.Add(new PageRevision
            {
                Id = Guid.NewGuid(),
                PageId = page.Id,
                Title = page.Title,
                ContentJson = page.ContentJson,
                Version = page.Version,
                CreatedBy = userId,
                Source = request.CreateRevision ? "manual" : "autosave",
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await RebuildLinksAsync(page, ct);
        await _db.SaveChangesAsync(ct);

        await _hub.Clients.Group($"page:{page.Id}").SendAsync("PageSaved", new { page.Id, page.Version }, ct);

        return ("ok", new { page.Id, page.Version, page.UpdatedAtUtc, page.Title, page.ContentJson });
    }

    public async Task<IReadOnlyList<object>> GetBacklinksAsync(Guid id, CancellationToken ct)
    {
        return await (from link in _db.PageLinks.AsNoTracking()
                      join page in _db.Pages.AsNoTracking() on link.FromPageId equals page.Id
                      where link.ToPageId == id
                      select (object)new { page.Id, page.Title, page.Slug, page.UpdatedAtUtc })
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
        var revision = await _db.PageRevisions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == revisionId && x.PageId == pageId, ct);
        if (page is null || revision is null) return null;

        var userId = _userContext.GetUserId();
        page.Title = revision.Title;
        page.Slug = Slugify(revision.Title);
        page.ContentJson = revision.ContentJson;
        page.Version += 1;
        page.UpdatedAtUtc = DateTime.UtcNow;
        page.UpdatedBy = userId;

        _db.PageRevisions.Add(new PageRevision
        {
            Id = Guid.NewGuid(),
            PageId = page.Id,
            Title = page.Title,
            ContentJson = page.ContentJson,
            Version = page.Version,
            CreatedBy = userId,
            Source = "restore",
            CreatedAtUtc = DateTime.UtcNow
        });

        await RebuildLinksAsync(page, ct);
        await _db.SaveChangesAsync(ct);

        return new
        {
            page.Id,
            page.Title,
            page.Version,
            page.ContentJson,
            page.UpdatedAtUtc,
            restoredFromRevisionId = revisionId
        };
    }

    public async Task<PageComment> AddCommentAsync(Guid pageId, AddCommentRequest request, CancellationToken ct)
    {
        var comment = new PageComment
        {
            Id = Guid.NewGuid(),
            PageId = pageId,
            AnchorJson = request.AnchorJson,
            Text = request.Text,
            AuthorId = _userContext.GetUserId(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.PageComments.Add(comment);
        await _db.SaveChangesAsync(ct);
        return comment;
    }

    public async Task<IReadOnlyList<PageComment>> GetCommentsAsync(Guid pageId, CancellationToken ct)
        => await _db.PageComments.AsNoTracking().Where(x => x.PageId == pageId).OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct);

    private async Task RebuildLinksAsync(Page page, CancellationToken ct)
    {
        var oldLinks = _db.PageLinks.Where(x => x.FromPageId == page.Id);
        _db.PageLinks.RemoveRange(oldLinks);

        var links = _linkParser.Parse(page.ContentJson);
        foreach (var link in links.DistinctBy(x => x.PageId))
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

        await Task.CompletedTask;
    }

    private static string Slugify(string title) => title.Trim().ToLowerInvariant().Replace(' ', '-');
}
