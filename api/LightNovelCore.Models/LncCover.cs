namespace LightNovelCore.Models;

/// <summary>
/// The cover URLs for a volume
/// </summary>
[Table("lnc_covers")]
[InterfaceOption(nameof(LncCover))]
public class LncCover : LncDbObject
{
	/// <summary>
	/// The ISBN the cover belongs to
	/// </summary>
	[Column("isbn", Unique = true)]
	[JsonPropertyName("isbn"), Required, MinLength(1)]
	public string Isbn { get; set; } = string.Empty;

	/// <summary>
	/// The URL of the cover image
	/// </summary>
	[Column("cover_url")]
	[JsonPropertyName("coverUrl"), Url]
	public string? CoverUrl { get; set; }

	/// <summary>
	/// The last time the image failed to be fetched
	/// </summary>
	[Column("last_failed_at")]
	[JsonIgnore]
	public DateTime? LastFailedAt { get; set; }

	/// <summary>
	/// The reason the image failed to be fetched
	/// </summary>
	[Column("failed_reason")]
	[JsonIgnore]
	public string? FailedReason { get; set; }

	/// <summary>
	/// The number of times the image has failed to be fetched
	/// </summary>
	[Column("failed_count")]
	[JsonIgnore]
	public int FailedCount { get; set; } = 0;

	/// <summary>
	/// The file name of the image
	/// </summary>
	[Column("file_name")]
	[JsonPropertyName("fileName")]
	public string? FileName { get; set; }

	/// <summary>
	/// A hash of the URL
	/// </summary>
	[Column("url_hash")]
	[JsonPropertyName("urlHash")]
	public string? UrlHash { get; set; }

	/// <summary>
	/// The width of the image in pixels
	/// </summary>
	[Column("image_width")]
	[JsonPropertyName("imageWidth")]
	public int? ImageWidth { get; set; }

	/// <summary>
	/// The height of the image in pixels
	/// </summary>
	[Column("image_height")]
	[JsonPropertyName("imageHeight")]
	public int? ImageHeight { get; set; }

	/// <summary>
	/// The size of the image in bytes
	/// </summary>
	[Column("image_size")]
	[JsonPropertyName("imageSize")]
	public long? ImageSize { get; set; }

	/// <summary>
	/// The mime-type of the image
	/// </summary>
	[Column("mime_type")]
	[JsonPropertyName("mimeType")]
	public string? MimeType { get; set; }
}
