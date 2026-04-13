using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WikiLive.Api.Contracts.Mws;

namespace WikiLive.Api.Services;

public sealed class MwsOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public sealed class MwsTablesClient : IMwsTablesClient
{
    private readonly HttpClient _httpClient;
    private readonly MwsOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public MwsTablesClient(HttpClient httpClient, IOptions<MwsOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
            _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");

        if (!string.IsNullOrWhiteSpace(_options.Token))
        {
            var token = _options.Token.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public Task<GetSpacesResponseDto> GetSpacesAsync(CancellationToken ct)
        => SendAsync<GetSpacesResponseDto>(HttpMethod.Get, "fusion/v1/spaces", null, ct);

    public Task<GetSpaceNodesResponseDto> GetSpaceNodesAsync(string spaceId, CancellationToken ct)
        => SendAsync<GetSpaceNodesResponseDto>(HttpMethod.Get, $"fusion/v1/spaces/{spaceId}/nodes", null, ct);

    public async Task<GetDatasheetsResponseDto> GetDatasheetsAsync(string spaceId, CancellationToken ct)
    {
        var nodes = await GetSpaceNodesAsync(spaceId, ct);

        var result = new GetDatasheetsResponseDto();

        foreach (var item in nodes.Data.Nodes)
        {
            var type = item.Type ?? item.NodeType;
            var id = !string.IsNullOrWhiteSpace(item.DstId) ? item.DstId : item.Id;

            if (string.Equals(type, "datasheet", StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(id) && id.StartsWith("dst", StringComparison.OrdinalIgnoreCase)))
            {
                result.Data.Datasheets.Add(new DatasheetNodeDto
                {
                    Id = item.Id,
                    DstId = item.DstId,
                    Name = item.Name,
                    Title = item.Title,
                    Type = item.Type,
                    NodeType = item.NodeType
                });
            }
        }

        return result;
    }

    public Task<CreateDatasheetResponseDto> CreateDatasheetAsync(
        string spaceId,
        CreateDatasheetRequestDto request,
        CancellationToken ct)
        => SendAsync<CreateDatasheetResponseDto>(
            HttpMethod.Post,
            $"fusion/v1/spaces/{spaceId}/datasheets",
            request,
            ct);

    public Task<DeleteDatasheetResponseDto> DeleteDatasheetAsync(string spaceId, string dstId, CancellationToken ct)
     => SendAsync<DeleteDatasheetResponseDto>(
         HttpMethod.Delete,
         $"fusion/v1/spaces/{spaceId}/datasheet/{dstId}",
         null,
         ct);

    public Task<GetRecordsResponseDto> GetRecordsAsync(
        string dstId,
        string? viewId,
        int? pageSize,
        int? pageNum,
        int? maxRecords,
        string? cellFormat,
        string? fieldKey,
        CancellationToken ct)
    {
        var query = BuildQuery(new Dictionary<string, string?>
        {
            ["viewId"] = viewId,
            ["pageSize"] = pageSize?.ToString(),
            ["pageNum"] = pageNum?.ToString(),
            ["maxRecords"] = maxRecords?.ToString(),
            ["cellFormat"] = cellFormat,
            ["fieldKey"] = fieldKey
        });

        return SendAsync<GetRecordsResponseDto>(
            HttpMethod.Get,
            $"fusion/v1/datasheets/{dstId}/records{query}",
            null,
            ct);
    }

    public Task<MutateRecordsResponseDto> CreateRecordsAsync(string dstId, CreateRecordsRequestDto request, CancellationToken ct)
        => SendAsync<MutateRecordsResponseDto>(
            HttpMethod.Post,
            $"fusion/v1/datasheets/{dstId}/records",
            request,
            ct);

    public Task<MutateRecordsResponseDto> UpdateRecordsAsync(string dstId, UpdateRecordsRequestDto request, CancellationToken ct)
        => SendAsync<MutateRecordsResponseDto>(
            HttpMethod.Patch,
            $"fusion/v1/datasheets/{dstId}/records",
            request,
            ct);

    public Task<MutateRecordsResponseDto> DeleteRecordsAsync(string dstId, DeleteRecordsRequestDto request, CancellationToken ct)
        => SendAsync<MutateRecordsResponseDto>(
            HttpMethod.Delete,
            $"fusion/v1/datasheets/{dstId}/records",
            request,
            ct);

    public Task<GetTimemachineResponseDto> GetTimemachineAsync(string dstId, CancellationToken ct)
     => SendAsync<GetTimemachineResponseDto>(
         HttpMethod.Get,
         $"fusion/v1/timemachine/{dstId}",
         null,
         ct);

    public Task<GetFieldsResponseDto> GetFieldsAsync(string dstId, string? viewId, CancellationToken ct)
    {
        var query = BuildQuery(new Dictionary<string, string?>
        {
            ["viewId"] = viewId
        });

        return SendAsync<GetFieldsResponseDto>(
            HttpMethod.Get,
            $"fusion/v1/datasheets/{dstId}/fields{query}",
            null,
            ct);
    }

    public Task<CreateFieldResponseDto> CreateFieldAsync(string spaceId, string dstId, CreateFieldRequestDto request, CancellationToken ct)
        => SendAsync<CreateFieldResponseDto>(
            HttpMethod.Post,
            $"fusion/v1/spaces/{spaceId}/datasheets/{dstId}/fields",
            request,
            ct);

    public Task<DeleteFieldResponseDto> DeleteFieldAsync(string spaceId, string dstId, string fieldId, CancellationToken ct)
    => SendAsync<DeleteFieldResponseDto>(
        HttpMethod.Delete,
        $"fusion/v1/spaces/{spaceId}/datasheets/{dstId}/fields/{fieldId}",
        null,
        ct);

    public Task<MoveFieldResponseDto> MoveFieldAsync(string dstId, string viewId, string fieldId, MoveFieldRequestDto request, CancellationToken ct)
        => SendAsync<MoveFieldResponseDto>(
            HttpMethod.Patch,
            $"fusion/v1/datasheets/{dstId}/views/{viewId}/fields/{fieldId}",
            request,
            ct);

    public Task<GetViewsResponseDto> GetViewsAsync(string dstId, CancellationToken ct)
        => SendAsync<GetViewsResponseDto>(
            HttpMethod.Get,
            $"fusion/v1/datasheets/{dstId}/views",
            null,
            ct);

    public Task<MutateViewResponseDto> CreateViewAsync(string spaceId, string dstId, CreateViewRequestDto request, CancellationToken ct)
        => SendAsync<MutateViewResponseDto>(
            HttpMethod.Post,
            $"fusion/v1/spaces/{spaceId}/datasheets/{dstId}/views",
            request,
            ct);

    public Task<MutateViewResponseDto> UpdateViewAsync(string spaceId, string dstId, string viewId, UpdateViewRequestDto request, CancellationToken ct)
        => SendAsync<MutateViewResponseDto>(
            HttpMethod.Put,
            $"fusion/v1/spaces/{spaceId}/datasheets/{dstId}/views/{viewId}",
            request,
            ct);

    public Task<DeleteViewResponseDto> DeleteViewAsync(string spaceId, string dstId, string viewId, CancellationToken ct)
        => SendAsync<DeleteViewResponseDto>(
            HttpMethod.Delete,
            $"fusion/v1/spaces/{spaceId}/datasheets/{dstId}/views/{viewId}",
            null,
            ct);

    public Task<MutateViewResponseDto> SortViewAsync(string spaceId, string dstId, string viewId, SortViewRequestDto request, CancellationToken ct)
        => SendAsync<MutateViewResponseDto>(
            HttpMethod.Post,
            $"fusion/v1/spaces/{spaceId}/datasheets/{dstId}/views/{viewId}/sort",
            request,
            ct);

    public Task<MutateViewResponseDto> GroupViewAsync(string spaceId, string dstId, string viewId, GroupViewRequestDto request, CancellationToken ct)
        => SendAsync<MutateViewResponseDto>(
            HttpMethod.Post,
            $"fusion/v1/spaces/{spaceId}/datasheets/{dstId}/views/{viewId}/group",
            request,
            ct);

    public Task<MutateViewResponseDto> HideFieldsAsync(string spaceId, string dstId, string viewId, HideFieldsRequestDto request, CancellationToken ct)
        => SendAsync<MutateViewResponseDto>(
            HttpMethod.Post,
            $"fusion/v1/spaces/{spaceId}/datasheets/{dstId}/views/{viewId}/hidden",
            request,
            ct);

    public Task<MutateViewResponseDto> MoveViewAsync(string spaceId, string dstId, string viewId, MoveViewRequestDto request, CancellationToken ct)
        => SendAsync<MutateViewResponseDto>(
            HttpMethod.Post,
            $"fusion/v1/spaces/{spaceId}/datasheets/{dstId}/views/{viewId}/move",
            request,
            ct);

    public async Task<byte[]> DownloadAttachmentAsync(string dstId, string token, CancellationToken ct)
    {
        using var response = await _httpClient.GetAsync(
            $"fusion/v1/datasheets/{dstId}/attachments?token={Uri.EscapeDataString(token)}",
            ct);

        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadAsByteArrayAsync(ct);
    }

    public async Task<UploadAttachmentResponseDto> UploadAttachmentAsync(string dstId, HttpContent content, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"fusion/v1/datasheets/{dstId}/attachments")
        {
            Content = content
        };

        using var response = await _httpClient.SendAsync(request, ct);
        await EnsureSuccessAsync(response, ct);

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var result = await JsonSerializer.DeserializeAsync<UploadAttachmentResponseDto>(stream, JsonOptions, ct);
        return result ?? new UploadAttachmentResponseDto();
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string url, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, url);

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        using var response = await _httpClient.SendAsync(request, ct);
        await EnsureSuccessAsync(response, ct);

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var result = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, ct);

        if (result is null)
            throw new Exception($"Empty response for {url}");

        return result;
    }

    public async Task<DownloadAttachmentMetadataDto> GetAttachmentMetadataAsync(string dstId, string token, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Head,
            $"fusion/v1/datasheets/{dstId}/attachments?token={Uri.EscapeDataString(token)}");

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        await EnsureSuccessAsync(response, ct);

        return new DownloadAttachmentMetadataDto
        {
            FileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                       ?? response.Content.Headers.ContentDisposition?.FileNameStar,
            MimeType = response.Content.Headers.ContentType?.MediaType,
            Size = response.Content.Headers.ContentLength
        };
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;

        var body = response.Content == null
            ? string.Empty
            : await response.Content.ReadAsStringAsync(ct);

        throw new HttpRequestException(
            $"FUSION API error: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}");
    }

    private static string BuildQuery(Dictionary<string, string?> parameters)
    {
        var parts = parameters
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}")
            .ToArray();

        return parts.Length == 0 ? string.Empty : "?" + string.Join("&", parts);
    }
}