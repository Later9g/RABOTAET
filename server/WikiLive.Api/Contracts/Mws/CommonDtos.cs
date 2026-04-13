namespace WikiLive.Api.Contracts.Mws;

public sealed class BaseMwsResponseDto
{
    public object? Data { get; set; }
    public bool? Success { get; set; }
    public string? Message { get; set; }
}

public sealed class DeleteDatasheetResponseDto
{
    public object? Data { get; set; }
    public bool? Success { get; set; }
    public string? Message { get; set; }
}

public sealed class DeleteFieldResponseDto
{
    public object? Data { get; set; }
    public bool? Success { get; set; }
    public string? Message { get; set; }
}

public sealed class DeleteViewResponseDto
{
    public object? Data { get; set; }
    public bool? Success { get; set; }
    public string? Message { get; set; }
}

public sealed class MoveFieldResponseDto
{
    public object? Data { get; set; }
    public bool? Success { get; set; }
    public string? Message { get; set; }
}