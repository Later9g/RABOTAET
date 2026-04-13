namespace WikiLive.Api.Contracts.Mws;

public sealed class GetTimemachineResponseDto
{
    public TimemachineDataDto? Data { get; set; }
    public bool? Success { get; set; }
    public string? Message { get; set; }
}

public sealed class TimemachineDataDto
{
    public List<TimemachineRevisionDto> Revisions { get; set; } = new();
}

public sealed class TimemachineRevisionDto
{
    public string? Id { get; set; }
    public string? RevisionId { get; set; }
    public string? CreatedAt { get; set; }
    public string? UpdatedAt { get; set; }
    public string? UserId { get; set; }
    public string? Action { get; set; }
}