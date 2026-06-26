using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Models.Dtos;

/// <summary>Someone the current user is allowed to chat with (shares a class).</summary>
public record ChatContactDto(
    int UserId,
    string Name,
    string Role,
    int UnreadCount);

public record ChatMessageDto(
    int Id,
    int SenderUserId,
    int RecipientUserId,
    string Body,
    DateTime SentAt,
    bool IsRead,
    bool Mine)
{
    public static ChatMessageDto From(Message m, int meUserId) => new(
        m.Id, m.SenderUserId, m.RecipientUserId, m.Body, m.SentAt, m.IsRead,
        m.SenderUserId == meUserId);
}
