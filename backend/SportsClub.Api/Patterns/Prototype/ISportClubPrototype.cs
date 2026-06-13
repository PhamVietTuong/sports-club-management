namespace SportsClub.Api.Patterns.Prototype;

/// <summary>
/// PROTOTYPE PATTERN — interface for all cloneable domain objects.
/// Allows duplicating a domain object without depending on its concrete type.
/// Concrete entities implement <see cref="Clone"/> with a shallow copy
/// (typically via <c>MemberwiseClone()</c>), mirroring the Java
/// <c>SportClubPrototype</c> / <c>Cloneable</c> design.
/// </summary>
/// <typeparam name="T">The concrete prototype type returned by Clone.</typeparam>
public interface ISportClubPrototype<out T>
{
    /// <summary>Returns a shallow copy of this object.</summary>
    T Clone();
}
