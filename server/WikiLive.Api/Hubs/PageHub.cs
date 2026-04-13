using Microsoft.AspNetCore.SignalR;
using WikiLive.Api.Contracts;

namespace WikiLive.Api.Hubs;

public class PageHub : Hub
{
    public async Task JoinPage(Guid pageId, string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"page:{pageId}");
        await Clients.OthersInGroup($"page:{pageId}")
            .SendAsync("PresenceJoined", new { pageId, userId });
    }

    public async Task LeavePage(Guid pageId, string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"page:{pageId}");
        await Clients.OthersInGroup($"page:{pageId}")
            .SendAsync("PresenceLeft", new { pageId, userId });
    }

    public async Task BroadcastPatch(Guid pageId, string userId, string contentJson, int version)
    {
        await Clients.OthersInGroup($"page:{pageId}")
            .SendAsync("PatchReceived", new
            {
                pageId,
                userId,
                contentJson,
                version
            });
    }

    // Comment events

    public async Task BroadcastCommentAdded(Guid pageId, CommentThreadDto comment)
    {
        await Clients.OthersInGroup($"page:{pageId}")
            .SendAsync("CommentAdded", comment);
    }

    public async Task BroadcastCommentReplyAdded(Guid pageId, CommentReplyDto reply)
    {
        await Clients.OthersInGroup($"page:{pageId}")
            .SendAsync("CommentReplyAdded", reply);
    }

    public async Task BroadcastCommentUpdated(Guid pageId, CommentThreadDto comment)
    {
        await Clients.OthersInGroup($"page:{pageId}")
            .SendAsync("CommentUpdated", comment);
    }

    public async Task BroadcastCommentReplyUpdated(Guid pageId, CommentReplyDto reply)
    {
        await Clients.OthersInGroup($"page:{pageId}")
            .SendAsync("CommentReplyUpdated", reply);
    }

    public async Task BroadcastCommentResolved(Guid pageId, CommentThreadDto comment)
    {
        await Clients.OthersInGroup($"page:{pageId}")
            .SendAsync("CommentResolved", comment);
    }

    public async Task BroadcastCommentDeleted(Guid pageId, Guid commentId)
    {
        await Clients.OthersInGroup($"page:{pageId}")
            .SendAsync("CommentDeleted", new { pageId, commentId });
    }
}