namespace NatechCats.Entities;

/// <summary>
/// Represents a tag entity.
/// </summary>
public class Tag
{
    /// <summary>
    /// The unique ID of the tag.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// The string that describes what this tag is about.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// The timestamp on when this record was inserted in our DB.
    /// </summary>
    public DateTime Created { get; set; }
    /// <summary>
    /// The cats that this tag is associated to.
    /// </summary>
    public ICollection<CatTag>? CatTags { get; set; }
}