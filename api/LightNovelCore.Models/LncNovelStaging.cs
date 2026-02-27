namespace LightNovelCore.Models;

using Types;

/// <summary>
/// The staging table for novel data
/// </summary>
[Table("lnc_novel_staging")]
public class LncNovelStaging : LncDbObject
{
	/// <summary>
	/// The series the book belongs to
	/// </summary>
	[Column("series")]
	[JsonPropertyName("series")]
	public string Series { get; set; } = string.Empty;

	/// <summary>
	/// The slug of the series the book belongs to
	/// </summary>
	[Column("series_slug")]
	[JsonPropertyName("seriesSlug")]
	public string SeriesSlug { get; set; } = string.Empty;

	/// <summary>
	/// The publisher of the book
	/// </summary>
	[Column("publisher")]
	[JsonPropertyName("publisher")]
	public string Publisher { get; set; } = string.Empty;

	/// <summary>
	/// The slug of the publisher of the book
	/// </summary>
	[Column("publisher_slug")]
	[JsonPropertyName("publisherSlug")]
	public string PublisherSlug { get; set; } = string.Empty;

	/// <summary>
	/// The URL of the series
	/// </summary>
	[Column("url")]
	[JsonPropertyName("url")]
	public string Url { get; set; } = string.Empty;

	/// <summary>
	/// The title of the novel
	/// </summary>
	[Column("title")]
	[JsonPropertyName("title")]
	public string Title { get; set; } = string.Empty;

	/// <summary>
	/// The volume number of the novel
	/// </summary>
	[Column("volume")]
	[JsonPropertyName("volume")]
	public string Volume { get; set; } = string.Empty;

	/// <summary>
	/// The publication format of the novel
	/// </summary>
	[Column("format")]
	[JsonPropertyName("format")]
	public LncFormat Format { get; set; }

	/// <summary>
	/// The book's ISBN, if it has one
	/// </summary>
	[Column("isbn")]
	[JsonPropertyName("isbn")]
	public string? ISBN { get; set; }

	/// <summary>
	/// The date the book was/is being published
	/// </summary>
	[Column("release_date")]
	[JsonPropertyName("releaseDate")]
	public DateTime ReleaseDate { get; set; }
}
