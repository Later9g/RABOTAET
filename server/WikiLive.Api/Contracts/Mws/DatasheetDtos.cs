namespace WikiLive.Api.Contracts.Mws;

public sealed class CreateDatasheetRequestDto
{
    public string Name { get; set; } = string.Empty;
}

public sealed class CreateDatasheetResponseDto
{
    public DatasheetDataDto Data { get; set; } = new();
}

public sealed class DatasheetDataDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Title { get; set; }
}

public sealed class GetDatasheetsResponseDto
{
    public DatasheetsContainerDto Data { get; set; } = new();
}

public sealed class DatasheetsContainerDto
{
    public List<DatasheetNodeDto> Datasheets { get; set; } = new();
}

public sealed class DatasheetNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string? DstId { get; set; }
    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? Type { get; set; }
    public string? NodeType { get; set; }
}