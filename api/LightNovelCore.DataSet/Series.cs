namespace LightNovelCore.DataSet;

/// <summary>
/// Represents a series
/// </summary>
[JsonConverter(typeof(ArrayConverter<Series>))]
public class Series
{
	/// <summary>
	/// The slug of the series
	/// </summary>
	[ArrayIndex(0)]
	public string Slug { get; set; } = string.Empty;

	/// <summary>
	/// The name of the series
	/// </summary>
	[ArrayIndex(1)]
	public string Name { get; set; } = string.Empty;
}
