using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Repositories;
using SportsClub.Api.Security;

namespace SportsClub.Api.Controllers;

/// <summary>
/// Direct messaging between a coach and the members in their classes. Shared by
/// both roles; you may only message someone you share a class with.
/// </summary>
[ApiController]
[Route("api/chat")]
[Authorize(Roles = $"{UserRole.Coach},{UserRole.Member}")]
public class ChatController : ControllerBase
{
    private readonly MemberRepository _members;
    private readonly CoachRepository _coaches;
    private readonly ClassRepository _classes;
    private readonly EnrollmentRepository _enrollments;
    private readonly MessageRepository _messages;

    public ChatController(MemberRepository members, CoachRepository coaches,
        ClassRepository classes, EnrollmentRepository enrollments, MessageRepository messages)
    {
        _members = members;
        _coaches = coaches;
        _classes = classes;
        _enrollments = enrollments;
        _messages = messages;
    }

    [HttpGet("contacts")]
    public async Task<ActionResult<IEnumerable<ChatContactDto>>> Contacts()
    {
        var contacts = await AllowedContactsAsync();
        var unread = await _messages.UnreadBySenderAsync(User.GetUserId());
        return Ok(contacts.Select(c =>
            new ChatContactDto(c.UserId, c.Name, c.Role, unread.GetValueOrDefault(c.UserId, 0))));
    }

    [HttpGet("conversation/{otherUserId:int}")]
    public async Task<IActionResult> Conversation(int otherUserId)
    {
        var meId = User.GetUserId();
        if (!await IsAllowedContactAsync(otherUserId))
            return StatusCode(StatusCodes.Status403Forbidden,
                new MessageResponse("Bạn không thể nhắn tin với người này."));

        await _messages.MarkReadAsync(meId, otherUserId);
        var msgs = await _messages.ConversationAsync(meId, otherUserId);
        return Ok(msgs.Select(m => ChatMessageDto.From(m, meId)));
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send(SendMessageRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Body))
            return BadRequest(new MessageResponse("Nội dung không được để trống."));
        if (req.Body.Length > 2000)
            return BadRequest(new MessageResponse("Tin nhắn quá dài (tối đa 2000 ký tự)."));
        if (!await IsAllowedContactAsync(req.RecipientUserId))
            return StatusCode(StatusCodes.Status403Forbidden,
                new MessageResponse("Bạn không thể nhắn tin với người này."));

        await _messages.SendAsync(User.GetUserId(), req.RecipientUserId, req.Body.Trim());
        return Ok(new MessageResponse("Đã gửi."));
    }

    private async Task<bool> IsAllowedContactAsync(int otherUserId) =>
        (await AllowedContactsAsync()).Any(c => c.UserId == otherUserId);

    /// <summary>
    /// AUTHORIZATION — the set of users the caller may chat with. A coach may
    /// message members actively enrolled in their classes; a member may message
    /// the coaches of classes they are actively enrolled in.
    /// </summary>
    private async Task<List<(int UserId, string Name, string Role)>> AllowedContactsAsync()
    {
        var meId = User.GetUserId();
        var result = new List<(int, string, string)>();

        if (User.IsInRole(UserRole.Coach))
        {
            var coach = await _coaches.FindByUserIdAsync(meId);
            if (coach is null) return result;
            var seen = new HashSet<int>();
            foreach (var c in await _classes.FindByCoachIdAsync(coach.Id))
            {
                foreach (var e in await _enrollments.FindActiveByClassIdAsync(c.Id))
                {
                    if (e.Member is not null && seen.Add(e.Member.UserId))
                        result.Add((e.Member.UserId, e.Member.FullName, UserRole.Member));
                }
            }
        }
        else // MEMBER
        {
            var member = await _members.FindByUserIdAsync(meId);
            if (member is null) return result;
            var coachIds = (await _enrollments.FindByMemberIdAsync(member.Id))
                .Where(e => e.Status == "ACTIVE" && e.Class?.CoachId != null)
                .Select(e => e.Class!.CoachId!.Value)
                .Distinct();
            foreach (var cid in coachIds)
            {
                var coach = await _coaches.FindByIdAsync(cid);
                if (coach is not null) result.Add((coach.UserId, coach.FullName, UserRole.Coach));
            }
        }
        return result;
    }
}
