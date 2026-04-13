using Microsoft.AspNetCore.Mvc;
using WikiLive.Api.Contracts.Mws;
using WikiLive.Api.Services;

namespace WikiLive.Api.Controllers;

[ApiController]
[Route("api/mws")]
public class MwsProxyController : ControllerBase
{
    private readonly IMwsTablesClient _client;

    public MwsProxyController(IMwsTablesClient client)
    {
        _client = client;
    }

    [HttpGet("spaces")]
    public async Task<IActionResult> GetSpaces(CancellationToken ct)
        => Ok(await _client.GetSpacesAsync(ct));

    [HttpGet("spaces/{spaceId}/nodes")]
    public async Task<IActionResult> GetSpaceNodes(string spaceId, CancellationToken ct)
        => Ok(await _client.GetSpaceNodesAsync(spaceId, ct));

    [HttpGet("spaces/{spaceId}/datasheets")]
    public async Task<IActionResult> GetDatasheets(string spaceId, CancellationToken ct)
        => Ok(await _client.GetDatasheetsAsync(spaceId, ct));

    [HttpPost("spaces/{spaceId}/datasheets")]
    public async Task<IActionResult> CreateDatasheet(
        string spaceId,
        [FromBody] CreateDatasheetRequestDto request,
        CancellationToken ct)
        => Ok(await _client.CreateDatasheetAsync(spaceId, request, ct));

    [HttpDelete("spaces/{spaceId}/datasheet/{dstId}")]
    public async Task<IActionResult> DeleteDatasheet(string spaceId, string dstId, CancellationToken ct)
    => Ok(await _client.DeleteDatasheetAsync(spaceId, dstId, ct));

    [HttpGet("datasheets/{dstId}/records")]
    public async Task<IActionResult> GetRecords(
        string dstId,
        [FromQuery] string? viewId,
        [FromQuery] int? pageSize,
        [FromQuery] int? pageNum,
        [FromQuery] int? maxRecords,
        [FromQuery] string? cellFormat,
        [FromQuery] string? fieldKey,
        CancellationToken ct)
        => Ok(await _client.GetRecordsAsync(dstId, viewId, pageSize, pageNum, maxRecords, cellFormat, fieldKey, ct));

    [HttpPost("datasheets/{dstId}/records")]
    public async Task<IActionResult> CreateRecords(
        string dstId,
        [FromBody] CreateRecordsRequestDto request,
        CancellationToken ct)
        => Ok(await _client.CreateRecordsAsync(dstId, request, ct));

    [HttpPatch("datasheets/{dstId}/records")]
    public async Task<IActionResult> UpdateRecords(
        string dstId,
        [FromBody] UpdateRecordsRequestDto request,
        CancellationToken ct)
        => Ok(await _client.UpdateRecordsAsync(dstId, request, ct));

    [HttpDelete("datasheets/{dstId}/records")]
    public async Task<IActionResult> DeleteRecords(
        string dstId,
        [FromBody] DeleteRecordsRequestDto request,
        CancellationToken ct)
        => Ok(await _client.DeleteRecordsAsync(dstId, request, ct));

    [HttpGet("timemachine/{dstId}")]
    public async Task<IActionResult> GetTimemachine(string dstId, CancellationToken ct)
    => Ok(await _client.GetTimemachineAsync(dstId, ct));

    [HttpGet("datasheets/{dstId}/fields")]
    public async Task<IActionResult> GetFields(string dstId, [FromQuery] string? viewId, CancellationToken ct)
        => Ok(await _client.GetFieldsAsync(dstId, viewId, ct));

    [HttpPost("spaces/{spaceId}/datasheets/{dstId}/fields")]
    public async Task<IActionResult> CreateField(
        string spaceId,
        string dstId,
        [FromBody] CreateFieldRequestDto request,
        CancellationToken ct)
        => Ok(await _client.CreateFieldAsync(spaceId, dstId, request, ct));

    [HttpDelete("spaces/{spaceId}/datasheets/{dstId}/fields/{fieldId}")]
    public async Task<IActionResult> DeleteField(string spaceId, string dstId, string fieldId, CancellationToken ct)
    => Ok(await _client.DeleteFieldAsync(spaceId, dstId, fieldId, ct));

    [HttpPatch("datasheets/{dstId}/views/{viewId}/fields/{fieldId}")]
    public async Task<IActionResult> MoveField(
        string dstId,
        string viewId,
        string fieldId,
        [FromBody] MoveFieldRequestDto request,
        CancellationToken ct)
        => Ok(await _client.MoveFieldAsync(dstId, viewId, fieldId, request, ct));

    [HttpGet("datasheets/{dstId}/views")]
    public async Task<IActionResult> GetViews(string dstId, CancellationToken ct)
        => Ok(await _client.GetViewsAsync(dstId, ct));

    [HttpPost("spaces/{spaceId}/datasheets/{dstId}/views")]
    public async Task<IActionResult> CreateView(
        string spaceId,
        string dstId,
        [FromBody] CreateViewRequestDto request,
        CancellationToken ct)
        => Ok(await _client.CreateViewAsync(spaceId, dstId, request, ct));

    [HttpPut("spaces/{spaceId}/datasheets/{dstId}/views/{viewId}")]
    public async Task<IActionResult> UpdateView(
        string spaceId,
        string dstId,
        string viewId,
        [FromBody] UpdateViewRequestDto request,
        CancellationToken ct)
        => Ok(await _client.UpdateViewAsync(spaceId, dstId, viewId, request, ct));

    [HttpDelete("spaces/{spaceId}/datasheets/{dstId}/views/{viewId}")]
    public async Task<IActionResult> DeleteView(string spaceId, string dstId, string viewId, CancellationToken ct)
        => Ok(await _client.DeleteViewAsync(spaceId, dstId, viewId, ct));

    [HttpPost("spaces/{spaceId}/datasheets/{dstId}/views/{viewId}/sort")]
    public async Task<IActionResult> SortView(
        string spaceId,
        string dstId,
        string viewId,
        [FromBody] SortViewRequestDto request,
        CancellationToken ct)
        => Ok(await _client.SortViewAsync(spaceId, dstId, viewId, request, ct));

    [HttpPost("spaces/{spaceId}/datasheets/{dstId}/views/{viewId}/group")]
    public async Task<IActionResult> GroupView(
        string spaceId,
        string dstId,
        string viewId,
        [FromBody] GroupViewRequestDto request,
        CancellationToken ct)
        => Ok(await _client.GroupViewAsync(spaceId, dstId, viewId, request, ct));

    [HttpPost("spaces/{spaceId}/datasheets/{dstId}/views/{viewId}/hidden")]
    public async Task<IActionResult> HideFields(
        string spaceId,
        string dstId,
        string viewId,
        [FromBody] HideFieldsRequestDto request,
        CancellationToken ct)
        => Ok(await _client.HideFieldsAsync(spaceId, dstId, viewId, request, ct));

    [HttpPost("spaces/{spaceId}/datasheets/{dstId}/views/{viewId}/move")]
    public async Task<IActionResult> MoveView(
        string spaceId,
        string dstId,
        string viewId,
        [FromBody] MoveViewRequestDto request,
        CancellationToken ct)
        => Ok(await _client.MoveViewAsync(spaceId, dstId, viewId, request, ct));

    [HttpGet("datasheets/{dstId}/attachments")]
    public async Task<IActionResult> DownloadAttachment(
        string dstId,
        [FromQuery] string token,
        CancellationToken ct)
    {
        var bytes = await _client.DownloadAttachmentAsync(dstId, token, ct);
        return File(bytes, "application/octet-stream");
    }

    [HttpPost("datasheets/{dstId}/attachments")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAttachment(
        string dstId,
        IFormFile file,
        CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required.");

        await using var stream = file.OpenReadStream();
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(stream);

        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType);

        form.Add(fileContent, "file", file.FileName);

        return Ok(await _client.UploadAttachmentAsync(dstId, form, ct));
    }

    [HttpHead("datasheets/{dstId}/attachments")]
    public async Task<IActionResult> GetAttachmentMetadata(
    string dstId,
    [FromQuery] string token,
    CancellationToken ct)
    {
        var metadata = await _client.GetAttachmentMetadataAsync(dstId, token, ct);

        if (!string.IsNullOrWhiteSpace(metadata.FileName))
            Response.Headers.ContentDisposition = $"attachment; filename=\"{metadata.FileName}\"";

        if (!string.IsNullOrWhiteSpace(metadata.MimeType))
            Response.ContentType = metadata.MimeType!;

        if (metadata.Size.HasValue)
            Response.ContentLength = metadata.Size.Value;

        return Ok(metadata);
    }
}