using Microsoft.AspNetCore.Http;

namespace WikiLive.Api.Services;

public interface IUserContext
{
    string GetUserId();
}

public class HeaderUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserId()
    {
        var ctx = _httpContextAccessor.HttpContext;
        var fromHeader = ctx?.Request.Headers["X-User-Id"].FirstOrDefault();
        return string.IsNullOrWhiteSpace(fromHeader) ? "demo-user" : fromHeader!;
    }
}
