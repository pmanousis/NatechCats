namespace NatechCats.Entities;

/// <summary>
/// Contains the association between cat and tag entities.
/// </summary>
public class CatTag
{
    /// <summary>
    /// The unique ID of the cat in the DB, foreign key relation to Cat entity.
    /// </summary>
    public int CatId { get; set; }
    /// <summary>
    /// The associated cat for this CatTag.
    /// </summary>
    public  Cat Cat { get; set; }
    /// <summary>
    /// The unique ID of the tag in the DB, foreign key relation to Tag entity.
    /// </summary>
    public int TagId { get; set; }
    /// <summary>
    /// The associated tag for this CatTag.
    /// </summary>
    public Tag Tag { get; set; }
}