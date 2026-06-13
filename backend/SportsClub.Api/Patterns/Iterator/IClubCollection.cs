namespace SportsClub.Api.Patterns.Iterator;

/// <summary>
/// ITERATOR PATTERN — aggregate interface. A collection knows how to create
/// an <see cref="IClubIterator{T}"/> over its elements without revealing how
/// those elements are stored.
/// </summary>
public interface IClubCollection<T>
{
    void Add(T item);
    int Count { get; }
    IClubIterator<T> CreateIterator();
}
