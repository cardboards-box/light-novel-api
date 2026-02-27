namespace LightNovelCore.Database.Services;

using Models;
using Models.Composites;

/// <summary>
/// The service for interacting with the lnc_volumes table
/// </summary>
public interface ILncVolumeDbService
{
    /// <summary>
    /// Fetches a record by its ID from the lnc_volumes table
    /// </summary>
    /// <param name="id">The ID of the record</param>
    /// <returns>The record</returns>
    Task<LncVolume?> Fetch(Guid id);

    /// <summary>
    /// Inserts a record into the lnc_volumes table
    /// </summary>
    /// <param name="item">The item to insert</param>
    /// <returns>The ID of the inserted record</returns>
    Task<Guid> Insert(LncVolume item);

    /// <summary>
    /// Updates a record in the lnc_volumes table
    /// </summary>
    /// <param name="item">The record to update</param>
    /// <returns>The number of records updated</returns>
    Task<int> Update(LncVolume item);

    /// <summary>
    /// Inserts a record in the lnc_volumes table if it doesn't exist, otherwise updates it
    /// </summary>
    /// <param name="item">The item to update or insert</param>
    /// <returns>The ID of the inserted/updated record</returns>
    Task<Guid> Upsert(LncVolume item);

    /// <summary>
    /// Gets all of the records from the lnc_volumes table
    /// </summary>
    /// <returns>All of the records</returns>
    Task<LncVolume[]> Get();

    /// <summary>
    /// Fetches the record and all related records
    /// </summary>
    /// <param name="id">The ID of the record to fetch</param>
    /// <returns>The record and all related records</returns>
    Task<LncEntity<LncVolume>?> FetchWithRelationships(Guid id);
}

internal class LncVolumeDbService(
    IOrmService orm) : Orm<LncVolume>(orm), ILncVolumeDbService
{
    public async Task<LncEntity<LncVolume>?> FetchWithRelationships(Guid id)
    {
        const string QUERY = @"SELECT * FROM lnc_volumes WHERE id = :id AND deleted_at IS NULL;
SELECT p.* 
FROM lnc_series p
JOIN lnc_volumes c ON p.id = c.series_id
WHERE 
    c.id = :id AND
    c.deleted_at IS NULL AND
    p.deleted_at IS NULL;
";
        using var con = await _sql.CreateConnection();
        using var rdr = await con.QueryMultipleAsync(QUERY, new { id });

        var item = await rdr.ReadSingleOrDefaultAsync<LncVolume>();
        if (item is null) return null;

        var related = new List<LncRelationship>();
        LncRelationship.Apply(related, await rdr.ReadAsync<LncSeries>());

        return new LncEntity<LncVolume>(item, [..related]);
    }
}