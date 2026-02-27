namespace LightNovelCore.Database.Services;

using Models;
using Models.Composites;

/// <summary>
/// The service for interacting with the lnc_publications table
/// </summary>
public interface ILncPublicationDbService
{
    /// <summary>
    /// Fetches a record by its ID from the lnc_publications table
    /// </summary>
    /// <param name="id">The ID of the record</param>
    /// <returns>The record</returns>
    Task<LncPublication?> Fetch(Guid id);

    /// <summary>
    /// Inserts a record into the lnc_publications table
    /// </summary>
    /// <param name="item">The item to insert</param>
    /// <returns>The ID of the inserted record</returns>
    Task<Guid> Insert(LncPublication item);

    /// <summary>
    /// Updates a record in the lnc_publications table
    /// </summary>
    /// <param name="item">The record to update</param>
    /// <returns>The number of records updated</returns>
    Task<int> Update(LncPublication item);

    /// <summary>
    /// Inserts a record in the lnc_publications table if it doesn't exist, otherwise updates it
    /// </summary>
    /// <param name="item">The item to update or insert</param>
    /// <returns>The ID of the inserted/updated record</returns>
    Task<Guid> Upsert(LncPublication item);

    /// <summary>
    /// Gets all of the records from the lnc_publications table
    /// </summary>
    /// <returns>All of the records</returns>
    Task<LncPublication[]> Get();

    /// <summary>
    /// Fetches the record and all related records
    /// </summary>
    /// <param name="id">The ID of the record to fetch</param>
    /// <returns>The record and all related records</returns>
    Task<LncEntity<LncPublication>?> FetchWithRelationships(Guid id);

    /// <summary>
    /// Searches for records with the given filters
    /// </summary>
    /// <param name="filter">The filter to use for searching</param>
    /// <returns>The paginated result of the search</returns>
    Task<PaginatedResult<LncEntity<LncPublication>>> Search(SearchFilter filter, bool skipLimit);
}

internal class LncPublicationDbService(
    IOrmService orm) : Orm<LncPublication>(orm), ILncPublicationDbService
{
    public async Task<LncEntity<LncPublication>?> FetchWithRelationships(Guid id)
    {
        const string QUERY = @"SELECT * FROM lnc_publications WHERE id = :id AND deleted_at IS NULL;
SELECT p.* 
FROM lnc_volumes p
JOIN lnc_publications c ON p.id = c.volume_id
WHERE 
    c.id = :id AND
    c.deleted_at IS NULL AND
    p.deleted_at IS NULL;

SELECT p.* 
FROM lnc_publishers p
JOIN lnc_publications c ON p.id = c.publisher_id
WHERE 
    c.id = :id AND
    c.deleted_at IS NULL AND
    p.deleted_at IS NULL;
";
        using var con = await _sql.CreateConnection();
        using var rdr = await con.QueryMultipleAsync(QUERY, new { id });

        var item = await rdr.ReadSingleOrDefaultAsync<LncPublication>();
        if (item is null) return null;

        var related = new List<LncRelationship>();
        LncRelationship.Apply(related, await rdr.ReadAsync<LncVolume>());
		LncRelationship.Apply(related, await rdr.ReadAsync<LncPublisher>());

        return new LncEntity<LncPublication>(item, [..related]);
    }

    public async Task<PaginatedResult<LncEntity<LncPublication>>> Search(SearchFilter filter, bool skipLimit)
    {
        var query = filter.BuildQuery(skipLimit, out var pars, out var size);
        using var con = await _sql.CreateConnection();
        using var rdr = await con.QueryMultipleAsync(query, pars);

		var total = await rdr.ReadSingleAsync<int>();
		if (total == 0) return new();

		var pages = (int)Math.Ceiling((double)total / size);
        var publishers = (await rdr.ReadAsync<LncPublisher>()).ToDictionary(t => t.Id);
        var series = (await rdr.ReadAsync<LncSeries>()).ToDictionary(t => t.Id);
        var volumes = (await rdr.ReadAsync<LncVolume>()).ToDictionary(t => t.Id);

        var publications = await rdr.ReadAsync<LncPublication>();
        var records = new List<LncEntity<LncPublication>>();

        foreach (var publication in publications)
        {
            var related = new List<LncRelationship>();
            if (publishers.TryGetValue(publication.PublisherId, out var publisher))
                LncRelationship.Apply(related, publisher);
            if (volumes.TryGetValue(publication.VolumeId, out var volume))
            {
                LncRelationship.Apply(related, volume);
                if (series.TryGetValue(volume.SeriesId, out var seri))
                    LncRelationship.Apply(related, seri);
			}

            records.Add(new(publication, [..related]));
		}

        return new(pages, total, [..records]);
	}
}