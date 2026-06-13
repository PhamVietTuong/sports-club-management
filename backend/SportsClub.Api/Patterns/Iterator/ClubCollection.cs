using System.Collections;

namespace SportsClub.Api.Patterns.Iterator;

/// <summary>
/// ITERATOR PATTERN — generic concrete collection. Wraps an internal list and
/// hands out a <see cref="ClubIterator{T}"/>. This single generic type replaces
/// the per-entity Java collections (MemberCollection, ClassCollection,
/// ScheduleCollection) while keeping identical semantics.
///
/// It implements <see cref="IEnumerable{T}"/> by enumerating through its own
/// <see cref="ClubIterator{T}"/>, so callers can do
/// <c>ClubCollection.Of(src).Select(...)</c> — the projection still flows
/// through the iterator, without materializing an extra intermediate list.
/// </summary>
public sealed class ClubCollection<T> : IClubCollection<T>, IEnumerable<T>
{
    private readonly List<T> _items = new();

    public void Add(T item) => _items.Add(item);

    public int Count => _items.Count;

    public IClubIterator<T> CreateIterator() => new ClubIterator<T>(_items);

    /// <summary>Builds a collection from an existing sequence in one call.</summary>
    public static ClubCollection<T> Of(IEnumerable<T> source)
    {
        var collection = new ClubCollection<T>();
        foreach (var item in source) collection.Add(item);
        return collection;
    }

    // Enumerate via the ClubIterator — this is the Iterator pattern in action
    // (the exact "while (it.hasNext()) ... it.next()" traversal the Java
    // servlets used), exposed as a standard IEnumerable so LINQ can consume it.
    public IEnumerator<T> GetEnumerator()
    {
        var iterator = CreateIterator();
        while (iterator.HasNext()) yield return iterator.Next();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
