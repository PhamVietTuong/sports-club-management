using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Repositories;
using SportsClub.Api.Security;
using SportsClub.Api.Services;

namespace SportsClub.Api.Controllers;

/// <summary>
/// Chat history + contacts over REST. Sending and live delivery happen over
/// SignalR (<c>ChatHub</c>); these endpoints load the initial state. You may
/// only see conversations with someone you share a class with.
/// </summary>
[ApiController]
[Route("api/chat")]
[Authorize(Roles = $"{UserRole.Coach},{UserRole.Member}")]
public class ChatController : ControllerBase
{
    private readonly MessageRepository _messages;
    private readonly ChatDirectory _directory;

    public ChatController(MessageRepository messages, ChatDirectory directory)
    {
        _messages = messages;
        _directory = directory;
    }

    [HttpGet("contacts")]
    public async Task<ActionResult<IEnumerable<ChatContactDto>>> Contacts()
    {
        var contacts = await _directory.ContactsAsync(User);
        var unread = await _messages.UnreadBySenderAsync(User.GetUserId());
        return Ok(contacts.Select(c =>
            new ChatContactDto(c.UserId, c.Name, c.Role, unread.GetValueOrDefault(c.UserId, 0))));
    }

    [HttpGet("conversation/{otherUserId:int}")]
    public async Task<IActionResult> Conversation(int otherUserId)
    {
        var meId = User.GetUserId();
        if (!await _directory.CanChatAsync(User, otherUserId))
            return StatusCode(StatusCodes.Status403Forbidden,
                new MessageResponse("Bạn không thể nhắn tin với người này."));

        await _messages.MarkReadAsync(meId, otherUserId);
        var msgs = await _messages.ConversationAsync(meId, otherUserId);
        return Ok(msgs.Select(m => ChatMessageDto.From(m, meId)));
    }
}
