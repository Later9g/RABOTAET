namespace WikiLive.Api.Contracts;

public record CreatePageRequest(string SpaceId, string Title);
public record UpdatePageRequest(string Title, string ContentJson, int Version, bool CreateRevision = false);
public record AddCommentRequest(string AnchorJson, string Text);
public record PageSearchResult(Guid Id, string Title, string Slug, DateTime UpdatedAtUtc);
public record LinkTarget(Guid PageId, string Title);
public record ResolveCommentRequest(bool Resolved);
public record UpdateCommentRequest(string Text);

public record CommentDto(
    Guid Id,
    Guid PageId,
    string AnchorJson,
    string Text,
    string AuthorId,
    DateTime CreatedAtUtc,
    bool IsResolved,
    string? ResolvedBy,
    DateTime? ResolvedAtUtc);

public record AddCommentReplyRequest(string Text);

public record CommentReplyDto(
    Guid Id,
    Guid PageId,
    Guid ParentCommentId,
    string Text,
    string AuthorId,
    DateTime CreatedAtUtc);

public record CommentThreadDto(
    Guid Id,
    Guid PageId,
    string AnchorJson,
    string Text,
    string AuthorId,
    DateTime CreatedAtUtc,
    bool IsResolved,
    string? ResolvedBy,
    DateTime? ResolvedAtUtc,
    IReadOnlyList<CommentReplyDto> Replies);