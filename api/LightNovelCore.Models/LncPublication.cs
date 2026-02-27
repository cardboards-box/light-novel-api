namespace LightNovelCore.Models;

using Types;

/// <summary>
/// All of the publications of volumes of novels
/// </summary>
[Table("lnc_publications")]
[InterfaceOption(nameof(LncPublication))]
public class LncPublication : LncDbObject
{
	/// <summary>
	/// The ID of the volume that this publication belongs to
	/// </summary>
	[Column("volume_id", Unique = true), Fk<LncVolume>]
	[JsonPropertyName("volumeId"), Required]
	public Guid VolumeId { get; set; }

	/// <summary>
	/// The unique hash for this record
	/// </summary>
	[Column("hash", Unique = true)]
	[JsonIgnore, Required]
	public string Hash { get; set; } = string.Empty;

	/// <summary>
	/// The ID of the publisher that published this volume
	/// </summary>
	[Column("publisher_id"), Fk<LncPublisher>]
	[JsonPropertyName("publisherId"), Required]
	public Guid PublisherId { get; set; }

	/// <summary>
	/// The format of the publication (e.g. physical, digital, etc.)
	/// </summary>
	[Column("format")]
	[JsonPropertyName("format"), Required]
	public LncFormat Format { get; set; }

	/// <summary>
	/// The ISBN for the publication
	/// </summary>
	[Column("isbn")]
	[JsonPropertyName("isbn")]
	public string? Isbn { get; set; }

	/// <summary>
	/// The URL to the publication
	/// </summary>
	[Column("url")]
	[JsonPropertyName("url")]
	public string? Url { get; set; }

	/// <summary>
	/// The release date of the publication
	/// </summary>
	[Column("release_date")]
	[JsonPropertyName("releaseDate"), Required]
	public DateTime ReleaseDate { get; set; }
}
