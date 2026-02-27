namespace LightNovelCore.Models;

/// <summary>
/// All of the volumes of a series
/// </summary>
[Table("lnc_volumes")]
[InterfaceOption(nameof(LncVolume))]
[Searchable(nameof(Title), nameof(Volume))]
public class LncVolume : LncDbObject
{
	/// <summary>
	/// The ID of the series this volume belongs to
	/// </summary>
	[Column("series_id", Unique = true), Fk<LncSeries>]
	[JsonPropertyName("seriesId"), Required]
	public Guid SeriesId { get; set; }

	/// <summary>
	/// The volume number
	/// </summary>
	[Column("volume", Unique = true)]
	[JsonPropertyName("volume"), MinLength(1), Required]
	public string Volume { get; set; } = string.Empty;

	/// <summary>
	/// The volume title
	/// </summary>
	[Column("title", Unique = true)]
	[JsonPropertyName("title"), MinLength(1), Required]
	public string Title { get; set; } = string.Empty;
}
