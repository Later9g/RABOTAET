namespace WikiLive.Api.Services;

public interface IUserContext
{
    string GetUserId();
}

public sealed class HeaderUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserId()
    {
        var value = _httpContextAccessor.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault();
        return string.IsNullOrWhiteSpace(value) ? "demo-user" : value!;
    }
}