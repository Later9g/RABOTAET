namespace WikiLive.Api.Contracts.Mws;

public sealed class GetSpacesResponseDto
{
    public SpacesDataDto Data { get; set; } = new();
}

public sealed class SpacesDataDto
{
    public List<SpaceDto> Spaces { get; set; } = new();
}

public sealed class SpaceDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
}

public sealed class GetSpaceNodesResponseDto
{
    public SpaceNodesDataDto Data { get; set; } = new();
}

public sealed class SpaceNodesDataDto
{
    public List<SpaceNodeDto> Nodes { get; set; } = new();
}

public sealed class SpaceNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? NodeType { get; set; }
    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? DstId { get; set; }
}