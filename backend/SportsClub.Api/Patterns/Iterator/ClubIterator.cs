namespace SportsClub.Api.Patterns.Iterator;

/// <summary>
/// ITERATOR PATTERN — concrete iterator. Walks the backing list by index,
/// exactly like the Java <c>MemberIterator</c> / <c>ScheduleIterator</c>.
/// </summary>
public sealed class ClubIterator<T> : IClubIterator<T>
{
    private readonly IReadOnlyList<T> _items;
    private int _index;

    public ClubIterator(IReadOnlyList<T> items) => _items = items;

    public bool HasNext() => _index < _items.Count;

    public T Next() => _items[_index++];

    public void Reset() => _index = 0;
}
