namespace WikiLive.Api.Contracts.Mws;

public sealed class GetViewsResponseDto
{
    public ViewsDataDto Data { get; set; } = new();
}

public sealed class ViewsDataDto
{
    public List<ViewDto> Views { get; set; } = new();
}

public sealed class ViewDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
}

public sealed class CreateViewRequestDto
{
    public string Name { get; set; } = string.Empty;
}

public sealed class UpdateViewRequestDto
{
    public string Name { get; set; } = string.Empty;
}

public sealed class SortViewRequestDto
{
    public List<object> Rules { get; set; } = new();
}

public sealed class GroupViewRequestDto
{
    public List<object> Rules { get; set; } = new();
}

public sealed class HideFieldsRequestDto
{
    public List<string> FieldIds { get; set; } = new();
}

public sealed class MoveViewRequestDto
{
    public int? PrevViewId { get; set; }
}

public sealed class MutateViewResponseDto
{
    public object? Data { get; set; }
}