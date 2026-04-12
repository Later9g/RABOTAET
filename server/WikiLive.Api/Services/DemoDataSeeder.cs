using System.Text.Json;
using WikiLive.Api.Domain;
using WikiLive.Api.Infrastructure;

namespace WikiLive.Api.Services;

public interface IDemoDataSeeder
{
    Task SeedAsync();
}

public class DemoDataSeeder : IDemoDataSeeder
{
    private readonly WikiDbContext _db;

    public DemoDataSeeder(WikiDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync()
    {
        if (_db.Pages.Any()) return;

        var page2 = new Page
        {
            Id = Guid.NewGuid(),
            SpaceId = "spc-demo",
            Title = "Отчёт по регионам",
            Slug = "отчёт-по-регионам",
            ContentJson = JsonSerializer.Serialize(new
            {
                type = "doc",
                content = new object[]
                {
                    new { type = "heading", attrs = new { level = 1 }, content = new [] { new { type = "text", text = "Отчёт по регионам" } } },
                    new { type = "paragraph", content = new [] { new { type = "text", text = "Страница для примера backlinks." } } }
                }
            }),
            CreatedBy = "seed",
            UpdatedBy = "seed"
        };

        var page1 = new Page
        {
            Id = Guid.NewGuid(),
            SpaceId = "spc-demo",
            Title = "План продаж",
            Slug = "план-продаж",
            ContentJson = JsonSerializer.Serialize(new
            {
                type = "doc",
                content = new object[]
                {
                    new { type = "heading", attrs = new { level = 1 }, content = new [] { new { type = "text", text = "План продаж" } } },
                    new { type = "paragraph", content = new [] { new { type = "text", text = "Главная wiki-страница с живой таблицей." } } },
                    new { type = "mwsTable", attrs = new { spaceId = "spc-demo", datasheetId = "dst-sales", title = "Продажи Q2", viewMode = "compact" } },
                    new { type = "paragraph", content = new object[] { new { type = "text", text = "См. также " }, new { type = "pageLink", attrs = new { pageId = page2.Id, title = page2.Title } } } }
                }
            }),
            CreatedBy = "seed",
            UpdatedBy = "seed"
        };

        _db.Pages.AddRange(page1, page2);
        _db.PageRevisions.AddRange(
            new PageRevision { Id = Guid.NewGuid(), PageId = page1.Id, Title = page1.Title, ContentJson = page1.ContentJson, Version = 1, Source = "seed", CreatedBy = "seed" },
            new PageRevision { Id = Guid.NewGuid(), PageId = page2.Id, Title = page2.Title, ContentJson = page2.ContentJson, Version = 1, Source = "seed", CreatedBy = "seed" }
        );
        _db.PageLinks.Add(new PageLink { Id = Guid.NewGuid(), FromPageId = page1.Id, ToPageId = page2.Id, LinkText = page2.Title });

        await _db.SaveChangesAsync();
    }
}
