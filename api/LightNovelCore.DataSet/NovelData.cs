namespace LightNovelCore.DataSet;

/// <summary>
/// Represents the data in the novel data set
/// </summary>
public class NovelData
{
	/// <summary>
	/// All of the series in the novel data set
	/// </summary>
	[JsonPropertyName("series")]
	public Series[] Series { get; set; } = [];

	/// <summary>
	/// All of the publishers in the novel data set
	/// </summary>
	[JsonPropertyName("publishers")]
	public string[] Publishers { get; set; } = [];

	/// <summary>
	/// All of the book data in the novel data set
	/// </summary>
	[JsonPropertyName("data")]
	public Book[] Data { get; set; } = [];

	/// <summary>
	/// A normalized version of the book data
	/// </summary>
	[JsonIgnore]
	public IEnumerable<Novel> Books => Data.Select(b => new Novel(b, this));
}
