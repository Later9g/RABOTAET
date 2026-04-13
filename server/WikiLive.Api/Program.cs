using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
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
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HeaderUserContext>();
builder.Services.AddSingleton<ILinkParserService, LinkParserService>();
builder.Services.AddScoped<IPageService, PageService>();

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
builder.Services.AddHttpClient<IMwsTablesClient, MwsTablesClient>((sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Mws:BaseUrl"];
    var token = cfg["Mws:Token"];

    if (!string.IsNullOrWhiteSpace(baseUrl))
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");

    if (!string.IsNullOrWhiteSpace(token))
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer",
                token.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase));
    }
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("client");

app.MapControllers();
app.MapHub<PageHub>("/hubs/page");

app.Run();