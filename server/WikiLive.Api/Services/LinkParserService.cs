using System.Text.Json;
using WikiLive.Api.Contracts;

namespace WikiLive.Api.Services;

public interface ILinkParserService
{
    IReadOnlyList<LinkTarget> Parse(string contentJson);
}

public class LinkParserService : ILinkParserService
{
    public IReadOnlyList<LinkTarget> Parse(string contentJson)
    {
        var results = new List<LinkTarget>();
        if (string.IsNullOrWhiteSpace(contentJson)) return results;

        using var doc = JsonDocument.Parse(contentJson);
        Traverse(doc.RootElement, results);
        return results;
    }

    private static void Traverse(JsonElement element, List<LinkTarget> results)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("type", out var typeEl) && typeEl.GetString() == "pageLink")
            {
                if (element.TryGetProperty("attrs", out var attrs))
                {
                    var pageIdText = attrs.TryGetProperty("pageId", out var pid) ? pid.GetString() : null;
                    var title = attrs.TryGetProperty("title", out var t) ? t.GetString() : null;
                    if (Guid.TryParse(pageIdText, out var pageId) && !string.IsNullOrWhiteSpace(title))
                    {
                        results.Add(new LinkTarget(pageId, title!));
                    }
                }
            }

            foreach (var property in element.EnumerateObject())
            {
                Traverse(property.Value, results);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                Traverse(item, results);
            }
        }
    }
}
