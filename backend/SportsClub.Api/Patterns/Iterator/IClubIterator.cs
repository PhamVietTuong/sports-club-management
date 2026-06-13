namespace SportsClub.Api.Patterns.Iterator;

/// <summary>
/// ITERATOR PATTERN — generic iterator interface for club collections.
/// Allows sequential traversal of a collection without exposing its
/// internal structure (a direct port of the Java <c>ClubIterator&lt;T&gt;</c>).
/// </summary>
public interface IClubIterator<out T>
{
    bool HasNext();
    T Next();
    void Reset();
}
