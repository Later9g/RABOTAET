namespace WikiLive.Api.Contracts;

public record CreatePageRequest(string SpaceId, string Title);
public record UpdatePageRequest(string Title, string ContentJson, int Version, bool CreateRevision = false);
public record AddCommentRequest(string AnchorJson, string Text);
public record PageSearchResult(Guid Id, string Title, string Slug, DateTime UpdatedAtUtc);
public record LinkTarget(Guid PageId, string Title);
