namespace LightNovelCore.Models;

/// <summary>
/// All of the publishers 
/// </summary>
[Table("lnc_publishers")]
[InterfaceOption(nameof(LncPublisher))]
[Searchable(nameof(Name))]
public partial class LncPublisher : LncDbObject
{
	/// <summary>
	/// The character to use to for slugs
	/// </summary>
	public const char SLUG = '-';

	/// <summary>
	/// The unique slug for the publisher
	/// </summary>
	[Column("slug", Unique = true)]
	[JsonPropertyName("slug"), Required, MinLength(1)]
	public string Slug
	{
		get => field ??= GenerateSlug(Name);
		set => field = GenerateSlug(value);
	}

	/// <summary>
	/// The name of the publisher
	/// </summary>
	[Column("name")]
	[JsonPropertyName("name"), Required, MinLength(1)]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// The URL to the publisher's icon
	/// </summary>
	[Column("icon_url")]
	[JsonPropertyName("iconUrl"), Url]
	public string? IconUrl { get; set; }

	/// <summary>
	/// The URL to the publisher's website
	/// </summary>
	[Column("website")]
	[JsonPropertyName("website"), Url]
	public string? Website { get; set; }

	/// <summary>
	/// Converts the given name to the appropriate slug
	/// </summary>
	/// <param name="name">The name to fix</param>
	/// <returns>The slug</returns>
	public static string GenerateSlug(string name)
	{
		name = NonAlphaNumericRegex().Replace(name, SLUG.ToString());
		while (name.Contains($"{SLUG}{SLUG}"))
			name = name.Replace($"{SLUG}{SLUG}", SLUG.ToString());
		return name.Trim(SLUG).ToLower();
	}

	[GeneratedRegex(@"[^a-zA-Z0-9]+")]
	private static partial Regex NonAlphaNumericRegex();
}
