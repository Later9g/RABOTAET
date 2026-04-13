using WikiLive.Api.Contracts.Mws;

namespace WikiLive.Api.Services;

public interface IMwsTablesClient
{
    Task<GetSpacesResponseDto> GetSpacesAsync(CancellationToken ct);
    Task<GetSpaceNodesResponseDto> GetSpaceNodesAsync(string spaceId, CancellationToken ct);
    Task<GetDatasheetsResponseDto> GetDatasheetsAsync(string spaceId, CancellationToken ct);

    Task<CreateDatasheetResponseDto> CreateDatasheetAsync(string spaceId, CreateDatasheetRequestDto request, CancellationToken ct);
    Task<DeleteDatasheetResponseDto> DeleteDatasheetAsync(string spaceId, string dstId, CancellationToken ct);

    Task<GetRecordsResponseDto> GetRecordsAsync(
        string dstId,
        string? viewId,
        int? pageSize,
        int? pageNum,
        int? maxRecords,
        string? cellFormat,
        string? fieldKey,
        CancellationToken ct);

    Task<MutateRecordsResponseDto> CreateRecordsAsync(string dstId, CreateRecordsRequestDto request, CancellationToken ct);
    Task<MutateRecordsResponseDto> UpdateRecordsAsync(string dstId, UpdateRecordsRequestDto request, CancellationToken ct);
    Task<MutateRecordsResponseDto> DeleteRecordsAsync(string dstId, DeleteRecordsRequestDto request, CancellationToken ct);

    Task<GetTimemachineResponseDto> GetTimemachineAsync(string dstId, CancellationToken ct);

    Task<GetFieldsResponseDto> GetFieldsAsync(string dstId, string? viewId, CancellationToken ct);
    Task<CreateFieldResponseDto> CreateFieldAsync(string spaceId, string dstId, CreateFieldRequestDto request, CancellationToken ct);
    Task<DeleteFieldResponseDto> DeleteFieldAsync(string spaceId, string dstId, string fieldId, CancellationToken ct);
    Task<MoveFieldResponseDto> MoveFieldAsync(string dstId, string viewId, string fieldId, MoveFieldRequestDto request, CancellationToken ct);

    Task<GetViewsResponseDto> GetViewsAsync(string dstId, CancellationToken ct);
    Task<MutateViewResponseDto> CreateViewAsync(string spaceId, string dstId, CreateViewRequestDto request, CancellationToken ct);
    Task<MutateViewResponseDto> UpdateViewAsync(string spaceId, string dstId, string viewId, UpdateViewRequestDto request, CancellationToken ct);
    Task<DeleteViewResponseDto> DeleteViewAsync(string spaceId, string dstId, string viewId, CancellationToken ct);
    Task<MutateViewResponseDto> SortViewAsync(string spaceId, string dstId, string viewId, SortViewRequestDto request, CancellationToken ct);
    Task<MutateViewResponseDto> GroupViewAsync(string spaceId, string dstId, string viewId, GroupViewRequestDto request, CancellationToken ct);
    Task<MutateViewResponseDto> HideFieldsAsync(string spaceId, string dstId, string viewId, HideFieldsRequestDto request, CancellationToken ct);
    Task<MutateViewResponseDto> MoveViewAsync(string spaceId, string dstId, string viewId, MoveViewRequestDto request, CancellationToken ct);

    Task<byte[]> DownloadAttachmentAsync(string dstId, string token, CancellationToken ct);
    Task<DownloadAttachmentMetadataDto> GetAttachmentMetadataAsync(string dstId, string token, CancellationToken ct);
    Task<UploadAttachmentResponseDto> UploadAttachmentAsync(string dstId, HttpContent content, CancellationToken ct);
}