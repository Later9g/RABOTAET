using Microsoft.AspNetCore.Mvc;
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

    [HttpGet("spaces/{spaceId}/datasheets")]
    public async Task<IActionResult> GetDatasheets(string spaceId, CancellationToken ct)
        => Ok(await _client.GetDatasheetsAsync(spaceId, ct));

    [HttpGet("datasheets/{dstId}/fields")]
    public async Task<IActionResult> GetFields(string dstId, [FromQuery] string? viewId, CancellationToken ct)
        => Ok(await _client.GetFieldsAsync(dstId, viewId, ct));

    [HttpGet("datasheets/{dstId}/views")]
    public async Task<IActionResult> GetViews(string dstId, CancellationToken ct)
        => Ok(await _client.GetViewsAsync(dstId, ct));

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
}
