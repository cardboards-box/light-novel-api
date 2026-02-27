namespace LightNovelCore.DataSet;

/// <summary>
/// Represents a novel
/// </summary>
[JsonConverter(typeof(ArrayConverter<Book>))]
public class Book
{
	/// <summary>
	/// The index of the series in the <see cref="NovelData.Series"/> array
	/// </summary>
	[ArrayIndex(0)]
	public int SeriesIndex { get; set; }

	/// <summary>
	/// The URL of the series
	/// </summary>
	[ArrayIndex(1)]
	public string Url { get; set; } = string.Empty;

	/// <summary>
	/// The index of the publisher in the <see cref="NovelData.Publishers"/> array
	/// </summary>
	[ArrayIndex(2)]
	public int PublisherIndex { get; set; }

	/// <summary>
	/// The title of the novel
	/// </summary>
	[ArrayIndex(3)]
	public string Title { get; set; } = string.Empty;

	/// <summary>
	/// The volume number of the novel
	/// </summary>
	[ArrayIndex(4)]
	public string Volume { get; set; } = string.Empty;

	/// <summary>
	/// The publication format of the novel
	/// </summary>
	[ArrayIndex(5)]
	public PublicationFormat Format { get; set; }

	/// <summary>
	/// The book's ISBN, if it has one
	/// </summary>
	[ArrayIndex(6)]
	public string? ISBN { get; set; }

	/// <summary>
	/// The date the book was/is being published
	/// </summary>
	[ArrayIndex(7)]
	public DateOnly Date { get; set; }
}
