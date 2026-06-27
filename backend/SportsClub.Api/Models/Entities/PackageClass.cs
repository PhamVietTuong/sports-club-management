namespace SportsClub.Api.Models.Entities;

/// <summary>
/// Join row linking a <see cref="TrainingPackage"/> to a <see cref="TrainingClass"/>
/// (maps to the <c>package_classes</c> table). A member who holds a package may
/// only register for the classes linked to it — e.g. the Standard package only
/// exposes the classes attached to it. Unique on (package_id, class_id).
/// </summary>
public class PackageClass
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public int ClassId { get; set; }
}
