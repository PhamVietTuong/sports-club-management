using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the messages table.</summary>
public class MessageRepository
{
    private readonly AppDbContext _db;
    public MessageRepository(AppDbContext db) => _db = db;

    /// <summary>The full conversation (both directions) between two users, oldest first.</summary>
    public Task<List<Message>> ConversationAsync(int userA, int userB) =>
        _db.Messages
            .Where(m => (m.SenderUserId == userA && m.RecipientUserId == userB)
                     || (m.SenderUserId == userB && m.RecipientUserId == userA))
            .OrderBy(m => m.SentAt).ThenBy(m => m.Id)
            .ToListAsync();

    /// <summary>Mark every message FROM <paramref name="otherUserId"/> TO me as read.</summary>
    public Task MarkReadAsync(int meUserId, int otherUserId) =>
        _db.Messages
            .Where(m => m.RecipientUserId == meUserId && m.SenderUserId == otherUserId && !m.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));

    /// <summary>Unread message counts grouped by the other party (sender), for my inbox.</summary>
    public async Task<Dictionary<int, int>> UnreadBySenderAsync(int meUserId)
    {
        var rows = await _db.Messages
            .Where(m => m.RecipientUserId == meUserId && !m.IsRead)
            .GroupBy(m => m.SenderUserId)
            .Select(g => new { SenderUserId = g.Key, Count = g.Count() })
            .ToListAsync();
        return rows.ToDictionary(r => r.SenderUserId, r => r.Count);
    }

    public async Task<Message> SendAsync(int senderUserId, int recipientUserId, string body)
    {
        var msg = new Message
        {
            SenderUserId = senderUserId,
            RecipientUserId = recipientUserId,
            Body = body,
            SentAt = DateTime.Now,
            IsRead = false,
        };
        _db.Messages.Add(msg);
        await _db.SaveChangesAsync();
        return msg;
    }
}
