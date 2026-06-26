using System.Security.Claims;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Repositories;
using SportsClub.Api.Security;

namespace SportsClub.Api.Services;

/// <summary>One user the caller is allowed to chat with.</summary>
public record ChatContactInfo(int UserId, string Name, string Role);

/// <summary>
/// Shared chat authorization: who a coach/member is allowed to message. Used by
/// both <c>ChatController</c> (REST history) and <c>ChatHub</c> (real-time send),
/// so the "shares an active class" rule lives in exactly one place.
/// </summary>
public class ChatDirectory
{
    private readonly MemberRepository _members;
    private readonly CoachRepository _coaches;
    private readonly ClassRepository _classes;
    private readonly EnrollmentRepository _enrollments;

    public ChatDirectory(MemberRepository members, CoachRepository coaches,
        ClassRepository classes, EnrollmentRepository enrollments)
    {
        _members = members;
        _coaches = coaches;
        _classes = classes;
        _enrollments = enrollments;
    }

    /// <summary>
    /// AUTHORIZATION — the users the caller may chat with. A coach may message
    /// members actively enrolled in their classes; a member may message the
    /// coaches of classes they are actively enrolled in.
    /// </summary>
    public async Task<List<ChatContactInfo>> ContactsAsync(ClaimsPrincipal user)
    {
        var meId = user.GetUserId();
        var result = new List<ChatContactInfo>();

        if (user.IsInRole(UserRole.Coach))
        {
            var coach = await _coaches.FindByUserIdAsync(meId);
            if (coach is null) return result;
            var seen = new HashSet<int>();
            foreach (var c in await _classes.FindByCoachIdAsync(coach.Id))
            {
                foreach (var e in await _enrollments.FindActiveByClassIdAsync(c.Id))
                {
                    if (e.Member is not null && seen.Add(e.Member.UserId))
                        result.Add(new ChatContactInfo(e.Member.UserId, e.Member.FullName, UserRole.Member));
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
                if (coach is not null) result.Add(new ChatContactInfo(coach.UserId, coach.FullName, UserRole.Coach));
            }
        }
        return result;
    }

    public async Task<bool> CanChatAsync(ClaimsPrincipal user, int otherUserId) =>
        (await ContactsAsync(user)).Any(c => c.UserId == otherUserId);
}
