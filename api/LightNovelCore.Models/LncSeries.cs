namespace LightNovelCore.Models;

/// <summary>
/// All of the series in the database.
/// </summary>
[Table("lnc_series")]
[InterfaceOption(nameof(LncSeries))]
[Searchable(nameof(Title))]
public class LncSeries : LncDbObject
{
	/// <summary>
	/// The unique slug for the series
	/// </summary>
	[Column("slug", Unique = true)]
	[JsonPropertyName("slug"), Required, MinLength(1)]
	public string Slug { get; set; } = string.Empty;

	/// <summary>
	/// The series' title
	/// </summary>
	[Column("title")]
	[JsonPropertyName("title"), MinLength(1)]
	public string Title { get; set; } = string.Empty;
}
