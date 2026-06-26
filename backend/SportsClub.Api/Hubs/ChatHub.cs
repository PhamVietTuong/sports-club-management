using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Repositories;
using SportsClub.Api.Security;
using SportsClub.Api.Services;

namespace SportsClub.Api.Hubs;

/// <summary>
/// Real-time coach ↔ member chat. Clients invoke <c>SendMessage</c>; the server
/// persists it and pushes a <c>ReceiveMessage</c> event to both the recipient and
/// the sender. SignalR's default user-id is the JWT's NameIdentifier (our user id),
/// so <c>Clients.User(id)</c> targets a specific person across all their tabs.
/// </summary>
[Authorize(Roles = $"{UserRole.Coach},{UserRole.Member}")]
public class ChatHub : Hub
{
    private readonly MessageRepository _messages;
    private readonly ChatDirectory _directory;

    public ChatHub(MessageRepository messages, ChatDirectory directory)
    {
        _messages = messages;
        _directory = directory;
    }

    public async Task SendMessage(int recipientUserId, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new HubException("Nội dung không được để trống.");
        if (body.Length > 2000)
            throw new HubException("Tin nhắn quá dài (tối đa 2000 ký tự).");

        // AUTHORIZATION — same "shares an active class" rule as the REST layer.
        if (!await _directory.CanChatAsync(Context.User!, recipientUserId))
            throw new HubException("Bạn không thể nhắn tin với người này.");

        var meId = Context.User!.GetUserId();
        Message saved = await _messages.SendAsync(meId, recipientUserId, body.Trim());

        // Push the raw fields; each client decides "mine" by comparing senderUserId.
        var payload = new
        {
            id = saved.Id,
            senderUserId = saved.SenderUserId,
            recipientUserId = saved.RecipientUserId,
            body = saved.Body,
            sentAt = saved.SentAt,
        };
        await Clients.User(recipientUserId.ToString()).SendAsync("ReceiveMessage", payload);
        await Clients.Caller.SendAsync("ReceiveMessage", payload);
    }
}
