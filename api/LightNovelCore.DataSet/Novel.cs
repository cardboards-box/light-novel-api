namespace LightNovelCore.DataSet;

using Models;

/// <summary>
/// A normalized version of <see cref="Book"/> with all the string indices resolved to their actual values
/// </summary>
public class Novel
{
	/// <summary>
	/// The series the book belongs to
	/// </summary>
	[JsonPropertyName("series")]
	public string Series { get; set; }

	/// <summary>
	/// The slug of the series the book belongs to
	/// </summary>
	[JsonPropertyName("seriesSlug")]
	public string SeriesSlug { get; set; }

	/// <summary>
	/// The publisher of the book
	/// </summary>
	[JsonPropertyName("publisher")]
	public string Publisher { get; set; }

	/// <summary>
	/// The slug of the publisher of the book
	/// </summary>
	[JsonPropertyName("publisherSlug")]
	public string PublisherSlug
	{
		get => field ??= LncPublisher.GenerateSlug(Publisher);
		set => field = LncPublisher.GenerateSlug(value);
	}

	/// <summary>
	/// The URL of the series
	/// </summary>
	[JsonPropertyName("url")]
	public string Url { get; set; }

	/// <summary>
	/// The title of the novel
	/// </summary>
	[JsonPropertyName("title")]
	public string Title { get; set; }

	/// <summary>
	/// The volume number of the novel
	/// </summary>
	[JsonPropertyName("volume")]
	public string Volume { get; set; }

	/// <summary>
	/// The publication format of the novel
	/// </summary>
	[JsonPropertyName("format")]
	public PublicationFormat Format { get; set; }

	/// <summary>
	/// The book's ISBN, if it has one
	/// </summary>
	[JsonPropertyName("isbn")]
	public string? ISBN
	{
		get => field?.Replace("-", "");
		set => field = value?.Replace("-", "");
	}

	/// <summary>
	/// The date the book was/is being published
	/// </summary>
	[JsonPropertyName("date")]
	public DateOnly Date { get; set; }

	internal Novel(Book _raw, NovelData _data)
	{
		Series = _data.Series[_raw.SeriesIndex].Name;
		SeriesSlug = _data.Series[_raw.SeriesIndex].Slug;
		Publisher = _data.Publishers[_raw.PublisherIndex];
		Url = _raw.Url;
		Title = _raw.Title;
		Volume = _raw.Volume;
		Format = _raw.Format;
		ISBN = _raw.ISBN;
		Date = _raw.Date;
	}

	[JsonConstructor]
	internal Novel()
	{
		Series = string.Empty;
		SeriesSlug = string.Empty;
		Publisher = string.Empty;
		Url = string.Empty;
		Title = string.Empty;
		Volume = string.Empty;
		Format = default;
		ISBN = string.Empty;
		Date = DateOnly.MinValue;
	}
}
