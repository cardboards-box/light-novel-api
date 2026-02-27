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
	/// The error message if there was an error
	/// </summary>
	[Column("error_message")]
	[JsonPropertyName("errorMessage")]
	public string? ErrorMessage { get; set; }
}
