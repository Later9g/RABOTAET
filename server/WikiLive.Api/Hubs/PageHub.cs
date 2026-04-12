using Microsoft.AspNetCore.SignalR;

namespace WikiLive.Api.Hubs;

public class PageHub : Hub
{
    public async Task JoinPage(Guid pageId, string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"page:{pageId}");
        await Clients.OthersInGroup($"page:{pageId}").SendAsync("PresenceJoined", new { pageId, userId, connectionId = Context.ConnectionId });
    }

    public async Task LeavePage(Guid pageId, string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"page:{pageId}");
        await Clients.OthersInGroup($"page:{pageId}").SendAsync("PresenceLeft", new { pageId, userId, connectionId = Context.ConnectionId });
    }

    public async Task BroadcastPatch(Guid pageId, string userId, string contentJson, int version)
    {
        await Clients.OthersInGroup($"page:{pageId}").SendAsync("PatchReceived", new { pageId, userId, contentJson, version });
    }

    public async Task UpdateCursor(Guid pageId, string userId, int from, int to)
    {
        await Clients.OthersInGroup($"page:{pageId}").SendAsync("CursorUpdated", new { pageId, userId, from, to });
    }
}
