namespace LightNovelCore.DataSet;

/// <summary>
/// The response back from the cover-image API
/// </summary>
public class CoverResponse
{
	/// <summary>
	/// The URL of the cover image
	/// </summary>
	[JsonPropertyName("url")]
	public string? Url { get; set; }

	/// <summary>
	/// The error message
	/// </summary>
	[JsonPropertyName("error")]
	public string? Error { get; set; }
}
