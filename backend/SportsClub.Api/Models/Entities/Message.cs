namespace SportsClub.Api.Models.Entities;

/// <summary>
/// A direct message between two users (maps to the <c>messages</c> table).
/// Used for coach ↔ member chat; keyed on user ids (not member/coach ids).
/// </summary>
public class Message
{
    public int Id { get; set; }
    public int SenderUserId { get; set; }
    public int RecipientUserId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }

    // Navigation
    public User Sender { get; set; } = null!;
    public User Recipient { get; set; } = null!;
}
