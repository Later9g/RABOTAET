namespace WikiLive.Api.Contracts.Mws;

public sealed class UploadAttachmentResponseDto
{
    public UploadAttachmentDataDto? Data { get; set; }
    public bool? Success { get; set; }
    public string? Message { get; set; }
}

public sealed class UploadAttachmentDataDto
{
    public string? Token { get; set; }
    public string? Url { get; set; }
    public string? MimeType { get; set; }
    public string? FileName { get; set; }
    public long? Size { get; set; }
}

public sealed class DownloadAttachmentMetadataDto
{
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public long? Size { get; set; }
}