namespace WikiLive.Api.Contracts.Mws;

public sealed class GetFieldsResponseDto
{
    public FieldsDataDto Data { get; set; } = new();
}

public sealed class FieldsDataDto
{
    public List<FieldDto> Fields { get; set; } = new();
}

public sealed class FieldDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Type { get; set; }
}

public sealed class CreateFieldRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public sealed class CreateFieldResponseDto
{
    public object? Data { get; set; }
}

public sealed class MoveFieldRequestDto
{
    public int? PrevFieldId { get; set; }
}