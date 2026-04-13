namespace WikiLive.Api.Contracts.Mws;

public sealed class GetRecordsResponseDto
{
    public RecordsDataDto Data { get; set; } = new();
}

public sealed class RecordsDataDto
{
    public List<RecordDto> Records { get; set; } = new();
}

public sealed class RecordDto
{
    public string? RecordId { get; set; }
    public Dictionary<string, object?> Fields { get; set; } = new();
}

public sealed class CreateRecordsRequestDto
{
    public List<CreateRecordDto> Records { get; set; } = new();
}

public sealed class CreateRecordDto
{
    public Dictionary<string, object?> Fields { get; set; } = new();
}

public sealed class UpdateRecordsRequestDto
{
    public List<UpdateRecordDto> Records { get; set; } = new();
}

public sealed class UpdateRecordDto
{
    public string RecordId { get; set; } = string.Empty;
    public Dictionary<string, object?> Fields { get; set; } = new();
}

public sealed class DeleteRecordsRequestDto
{
    public List<string> RecordIds { get; set; } = new();
}

public sealed class MutateRecordsResponseDto
{
    public object? Data { get; set; }
}