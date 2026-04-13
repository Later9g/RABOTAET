namespace WikiLive.Api.Domain;

public class Page
{
    public Guid Id { get; set; }
    public string SpaceId { get; set; } = "spc-demo";
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ContentJson { get; set; } = "{\"type\":\"doc\",\"content\":[]}";
    public int Version { get; set; } = 1;
    public string CreatedBy { get; set; } = "demo-user";
    public string UpdatedBy { get; set; } = "demo-user";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class PageRevision
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ContentJson { get; set; } = string.Empty;
    public int Version { get; set; }
    public string CreatedBy { get; set; } = "demo-user";
    public string Source { get; set; } = "autosave";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class PageLink
{
    public Guid Id { get; set; }
    public Guid FromPageId { get; set; }
    public Guid ToPageId { get; set; }
    public string LinkText { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}


public class PageComment
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }

    public Guid? ParentCommentId { get; set; }

    public string AnchorJson { get; set; } = "{}";

    public string Text { get; set; } = string.Empty;

    public string AuthorId { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public bool IsResolved { get; set; }
    public string? ResolvedBy { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
}

