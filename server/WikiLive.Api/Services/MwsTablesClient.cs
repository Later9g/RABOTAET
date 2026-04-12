using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace WikiLive.Api.Services;

public class MwsOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public bool UseMock { get; set; }
}

public record MwsSpaceDto(string Id, string Name);
public record MwsDatasheetDto(string Id, string SpaceId, string Title);

public interface IMwsTablesClient
{
    Task<IReadOnlyList<MwsSpaceDto>> GetSpacesAsync(CancellationToken ct);
    Task<IReadOnlyList<MwsDatasheetDto>> GetDatasheetsAsync(string spaceId, CancellationToken ct);
    Task<object> GetFieldsAsync(string dstId, string? viewId, CancellationToken ct);
    Task<object> GetViewsAsync(string dstId, CancellationToken ct);
    Task<object> GetRecordsAsync(string dstId, string? viewId, int? pageSize, int? pageNum, int? maxRecords, string? cellFormat, string? fieldKey, CancellationToken ct);
}

public class MwsTablesClient : IMwsTablesClient
{
    private readonly HttpClient _httpClient;
    private readonly MwsOptions _options;

    public MwsTablesClient(HttpClient httpClient, IOptions<MwsOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<MwsSpaceDto>> GetSpacesAsync(CancellationToken ct)
    {
        if (_options.UseMock)
        {
            return [new("spc-demo", "Demo Space")];
        }

        var doc = await SendJsonAsync(HttpMethod.Get, BuildUrl("/fusion/v1/spaces"), ct);
        return ExtractSpaces(doc.RootElement);
    }

    public async Task<IReadOnlyList<MwsDatasheetDto>> GetDatasheetsAsync(string spaceId, CancellationToken ct)
    {
        if (_options.UseMock)
        {
            return new List<MwsDatasheetDto>
            {
                new("dst-sales", spaceId, "Продажи Q2"),
                new("dst-hr", spaceId, "HR План"),
                new("dst-risks", spaceId, "Риски проекта")
            };
        }

        var doc = await SendJsonAsync(HttpMethod.Get, BuildUrl($"/fusion/v1/spaces/{spaceId}/nodes"), ct);
        return ExtractDatasheets(spaceId, doc.RootElement);
    }

    public async Task<object> GetFieldsAsync(string dstId, string? viewId, CancellationToken ct)
    {
        if (_options.UseMock)
        {
            return new
            {
                data = new
                {
                    fields = new object[]
                    {
                        new { id = "fld-region", name = "Регион" },
                        new { id = "fld-plan", name = "План" },
                        new { id = "fld-fact", name = "Факт" },
                        new { id = "fld-status", name = "Статус" }
                    }
                }
            };
        }

        var query = string.IsNullOrWhiteSpace(viewId) ? string.Empty : $"?viewId={Uri.EscapeDataString(viewId)}";
        var doc = await SendJsonAsync(HttpMethod.Get, BuildUrl($"/fusion/v1/datasheets/{dstId}/fields{query}"), ct);
        return JsonSerializer.Deserialize<object>(doc.RootElement.GetRawText())!;
    }

    public async Task<object> GetViewsAsync(string dstId, CancellationToken ct)
    {
        if (_options.UseMock)
        {
            return new
            {
                data = new
                {
                    views = new object[]
                    {
                        new { id = "viw-main", name = "Основное" }
                    }
                }
            };
        }

        var doc = await SendJsonAsync(HttpMethod.Get, BuildUrl($"/fusion/v1/datasheets/{dstId}/views"), ct);
        return JsonSerializer.Deserialize<object>(doc.RootElement.GetRawText())!;
    }

    public async Task<object> GetRecordsAsync(string dstId, string? viewId, int? pageSize, int? pageNum, int? maxRecords, string? cellFormat, string? fieldKey, CancellationToken ct)
    {
        if (_options.UseMock)
        {
            return dstId switch
            {
                "dst-sales" => new
                {
                    data = new
                    {
                        records = new object[]
                        {
                            new { recordId = "rec-1", fields = new Dictionary<string, object> { ["Регион"] = "EMEA", ["План"] = 1200000, ["Факт"] = 1170000, ["Статус"] = "В процессе" } },
                            new { recordId = "rec-2", fields = new Dictionary<string, object> { ["Регион"] = "APAC", ["План"] = 900000, ["Факт"] = 960000, ["Статус"] = "Перевыполнение" } },
                            new { recordId = "rec-3", fields = new Dictionary<string, object> { ["Регион"] = "LATAM", ["План"] = 300000, ["Факт"] = 250000, ["Статус"] = "Риск" } }
                        }
                    }
                },
                _ => new { data = new { records = Array.Empty<object>() } }
            };
        }

        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(viewId)) query.Add($"viewId={Uri.EscapeDataString(viewId)}");
        if (pageSize.HasValue) query.Add($"pageSize={pageSize.Value}");
        if (pageNum.HasValue) query.Add($"pageNum={pageNum.Value}");
        if (maxRecords.HasValue) query.Add($"maxRecords={maxRecords.Value}");
        if (!string.IsNullOrWhiteSpace(cellFormat)) query.Add($"cellFormat={Uri.EscapeDataString(cellFormat)}");
        if (!string.IsNullOrWhiteSpace(fieldKey)) query.Add($"fieldKey={Uri.EscapeDataString(fieldKey)}");

        var qs = query.Count == 0 ? string.Empty : $"?{string.Join("&", query)}";
        var doc = await SendJsonAsync(HttpMethod.Get, BuildUrl($"/fusion/v1/datasheets/{dstId}/records{qs}"), ct);
        return JsonSerializer.Deserialize<object>(doc.RootElement.GetRawText())!;
    }

    private async Task<JsonDocument> SendJsonAsync(HttpMethod method, string url, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, url);
        if (!string.IsNullOrWhiteSpace(_options.Token))
        {
            var token = _options.Token.Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var response = await _httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"MWS request failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}");
        }

        return JsonDocument.Parse(body);
    }

    private string BuildUrl(string path)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
            throw new InvalidOperationException("Mws:BaseUrl is not configured.");

        return $"{_options.BaseUrl.TrimEnd('/')}{path}";
    }

    private static IReadOnlyList<MwsSpaceDto> ExtractSpaces(JsonElement root)
    {
        var result = new List<MwsSpaceDto>();
        if (TryGetNestedArray(root, out var spaces, "data", "spaces"))
        {
            foreach (var item in spaces.EnumerateArray())
            {
                var id = GetString(item, "id");
                var name = GetString(item, "name") ?? id;
                if (!string.IsNullOrWhiteSpace(id))
                {
                    result.Add(new MwsSpaceDto(id!, name ?? id!));
                }
            }
        }
        return result;
    }

    private static IReadOnlyList<MwsDatasheetDto> ExtractDatasheets(string spaceId, JsonElement root)
    {
        var result = new List<MwsDatasheetDto>();
        if (TryGetNestedArray(root, out var nodes, "data", "nodes"))
        {
            foreach (var item in nodes.EnumerateArray())
            {
                var id = GetString(item, "id")
                         ?? GetString(item, "dstId")
                         ?? GetString(item, "datasheetId")
                         ?? GetString(item, "nodeId");

                var title = GetString(item, "name")
                            ?? GetString(item, "title")
                            ?? id;

                var looksLikeDatasheet = false;
                var typeText = GetString(item, "type")
                               ?? GetString(item, "nodeType")
                               ?? GetString(item, "kind");

                if (!string.IsNullOrWhiteSpace(typeText) && typeText!.Contains("datasheet", StringComparison.OrdinalIgnoreCase))
                {
                    looksLikeDatasheet = true;
                }

                if (!looksLikeDatasheet && !string.IsNullOrWhiteSpace(id) && id!.StartsWith("dst", StringComparison.OrdinalIgnoreCase))
                {
                    looksLikeDatasheet = true;
                }

                if (looksLikeDatasheet && !string.IsNullOrWhiteSpace(id))
                {
                    result.Add(new MwsDatasheetDto(id!, spaceId, title ?? id!));
                }
            }
        }
        return result;
    }

    private static bool TryGetNestedArray(JsonElement root, out JsonElement array, params string[] path)
    {
        array = root;
        foreach (var segment in path)
        {
            if (!array.TryGetProperty(segment, out array))
            {
                return false;
            }
        }

        return array.ValueKind == JsonValueKind.Array;
    }

    private static string? GetString(JsonElement element, string name)
    {
        if (element.TryGetProperty(name, out var prop))
        {
            return prop.ValueKind switch
            {
                JsonValueKind.String => prop.GetString(),
                JsonValueKind.Number => prop.GetRawText(),
                _ => null
            };
        }

        return null;
    }
}
