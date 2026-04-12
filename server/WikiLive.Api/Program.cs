using Microsoft.EntityFrameworkCore;
using WikiLive.Api.Hubs;
using WikiLive.Api.Infrastructure;
using WikiLive.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR().AddJsonProtocol();
builder.Services.AddCors(options =>
{
    options.AddPolicy("client", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HeaderUserContext>();

var useInMemory = builder.Configuration.GetValue<bool?>("Database:UseInMemory") ?? false;
var connectionString = builder.Configuration.GetConnectionString("Default");

if (!useInMemory && !string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<WikiDbContext>(options => options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<WikiDbContext>(options => options.UseInMemoryDatabase("wikilive"));
}

builder.Services.Configure<MwsOptions>(builder.Configuration.GetSection("Mws"));
builder.Services.AddHttpClient<IMwsTablesClient, MwsTablesClient>();
builder.Services.AddSingleton<ILinkParserService, LinkParserService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IDemoDataSeeder, DemoDataSeeder>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("client");
app.MapControllers();
app.MapHub<PageHub>("/hubs/page");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WikiDbContext>();
    if (db.Database.IsRelational())
    {
        await db.Database.EnsureCreatedAsync();
    }

    var seeder = scope.ServiceProvider.GetRequiredService<IDemoDataSeeder>();
    await seeder.SeedAsync();
}

app.Run();
