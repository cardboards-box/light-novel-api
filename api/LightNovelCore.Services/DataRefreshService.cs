namespace LightNovelCore.Services;

/// <summary>
/// A service for refreshing the novel data in the database
/// </summary>
public interface IDataRefreshService
{
	/// <summary>
	/// Loads all of the latest data into the database
	/// </summary>
	/// <param name="token">The cancellation token</param>
	/// <returns>The records that were newly created</returns>
	Task<Boxed> Load(CancellationToken token);
}

internal class DataRefreshService(
	IDbService _db,
	INovelApiService _api,
	ILogger<DataRefreshService> _logger) : IDataRefreshService
{
	public static IEnumerable<LncNovelStaging> Convert(IEnumerable<Novel> novels)
	{
		foreach(var novel in novels)
		{
			var formats = novel.Format.Flags();
			foreach (var format in formats)
				yield return new()
				{
					Series = novel.Series,
					SeriesSlug = novel.SeriesSlug,
					Publisher = novel.Publisher,
					PublisherSlug = novel.PublisherSlug,
					Url = novel.Url,
					Title = novel.Title,
					Volume = novel.Volume,
					Format = format switch 
					{ 
						PublicationFormat.Physical => LncFormat.Physical,
						PublicationFormat.Digital => LncFormat.Digital,
						_ => LncFormat.Audio
					},
					ISBN = novel.ISBN,
					ReleaseDate = novel.Date.ToDateTime(TimeOnly.MinValue)
				};
		}
	}

	public async Task<Boxed> Load(CancellationToken token)
	{
		try
		{
			var novels = await _api.Get();
			if (novels is null || novels.Length == 0)
				return Boxed.Exception("No novels were found from the API");

			_logger.LogInformation("Loading {count} novels into the database", novels.Length);
			var books = Convert(novels);
			await _db.Staging.BulkInsert(books, token);
			var results = await _db.Staging.Merge();
			_logger.LogInformation("Loaded New Items: {Publishers} publishers, {Series} series, {Volumes} volumes, {Publications} publications", 
				results.Publishers.Length, results.Series.Length, results.Volumes.Length, results.Publications.Length);
			return Boxed.Ok(results);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while loading data");
			return Boxed.Exception(ex);
		}
	}
}
