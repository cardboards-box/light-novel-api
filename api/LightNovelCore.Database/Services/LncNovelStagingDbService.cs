using Npgsql;
using NpgsqlTypes;

namespace LightNovelCore.Database.Services;

using LightNovelCore.Models.Composites;
using Models;

/// <summary>
/// The service for interacting with the lnc_novel_staging table
/// </summary>
public interface ILncNovelStagingDbService
{
	/// <summary>
	/// Bulk inserts the given items into the lnc_novel_staging table
	/// </summary>
	/// <param name="items">The items to insert</param>
	/// <param name="token">The cancellation token for the request</param>
	Task BulkInsert(IEnumerable<LncNovelStaging> items, CancellationToken token);

	/// <summary>
	/// Merge the latest staging data into the main tables
	/// </summary>
	Task<SeriesUpsertResult> Merge();
}

internal class LncNovelStagingDbService(
	IOrmService orm,
	IQueryCacheService _cache) : Orm<LncNovelStaging>(orm), ILncNovelStagingDbService
{
	public async Task BulkInsert(IEnumerable<LncNovelStaging> novels, CancellationToken token)
	{
		const string QUERY = @"COPY lnc_novel_staging (
    series,
    series_slug,
    publisher,
    publisher_slug,
    url,
    title,
    volume,
    format,
    isbn,
    release_date
)
FROM STDIN (FORMAT BINARY)";

		using var con = (NpgsqlConnection)await _sql.CreateConnection();
		await con.ExecuteAsync("TRUNCATE TABLE lnc_novel_staging", token);

		using var writer = await con.BeginBinaryImportAsync(QUERY, token);

		foreach (var novel in novels)
		{
			await writer.StartRowAsync(token);

			writer.Write(novel.Series, NpgsqlDbType.Text);
			writer.Write(novel.SeriesSlug, NpgsqlDbType.Text);
			writer.Write(novel.Publisher, NpgsqlDbType.Text);
			writer.Write(novel.PublisherSlug, NpgsqlDbType.Text);
			writer.Write(novel.Url, NpgsqlDbType.Text);
			writer.Write(novel.Title, NpgsqlDbType.Text);
			writer.Write(novel.Volume, NpgsqlDbType.Text);

			writer.Write((int)novel.Format, NpgsqlDbType.Integer);

			if (novel.ISBN is null) writer.WriteNull();
			else writer.Write(novel.ISBN, NpgsqlDbType.Text);

			writer.Write(novel.ReleaseDate, NpgsqlDbType.Timestamp);
		}

		await writer.CompleteAsync(token);
	}

	public async Task<SeriesUpsertResult> Merge()
	{
		var query = await _cache.Required("merge_staging");
		using var con = await _sql.CreateConnection();
		using var rdr = await con.QueryMultipleAsync(query);

		var publishers = await rdr.ReadAsync<LncPublisher>();
		var series = await rdr.ReadAsync<LncSeries>();
		var volumes = await rdr.ReadAsync<LncVolume>();
		var publications = await rdr.ReadAsync<LncPublication>();

		return new([..publishers], [..series], [..volumes], [..publications]);
	}
}
