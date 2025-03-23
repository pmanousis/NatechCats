namespace NatechCats.Entities;

/// <summary>
/// Represents a cat entity.
/// </summary>
public class Cat
{
    /// <summary>
    /// The unique ID of the cat.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// The ID of the cat from https://api.thecatapi.com/.
    /// </summary>
    public required string CatId { get; set; }
    /// <summary>
    /// Represents the width of the image returned from https://api.thecatapi.com/.
    /// </summary>
    public int Width { get; set; }
    /// <summary>
    /// Represents the height of the image returned from https://api.thecatapi.com/.
    /// </summary>
    public int Height { get; set; }
    /// <summary>
    /// Contains the image data returned from https://api.thecatapi.com/.
    /// </summary>
    public byte[]? Image { get; set; }
    /// <summary>
    /// The timestamp on when this record was inserted in our DB.
    /// </summary>
    public DateTime Created { get; set; }
    /// <summary>
    /// The tags that this cat is associated to.
    /// </summary>
    public ICollection<CatTag>? CatTags { get; set; }
}