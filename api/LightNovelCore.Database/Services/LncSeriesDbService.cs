namespace LightNovelCore.Database.Services;

using Models;
using Models.Composites;

/// <summary>
/// The service for interacting with the lnc_series table
/// </summary>
public interface ILncSeriesDbService
{
    /// <summary>
    /// Fetches a record by its ID from the lnc_series table
    /// </summary>
    /// <param name="id">The ID of the record</param>
    /// <returns>The record</returns>
    Task<LncSeries?> Fetch(Guid id);

    /// <summary>
    /// Inserts a record into the lnc_series table
    /// </summary>
    /// <param name="item">The item to insert</param>
    /// <returns>The ID of the inserted record</returns>
    Task<Guid> Insert(LncSeries item);

    /// <summary>
    /// Updates a record in the lnc_series table
    /// </summary>
    /// <param name="item">The record to update</param>
    /// <returns>The number of records updated</returns>
    Task<int> Update(LncSeries item);

    /// <summary>
    /// Inserts a record in the lnc_series table if it doesn't exist, otherwise updates it
    /// </summary>
    /// <param name="item">The item to update or insert</param>
    /// <returns>The ID of the inserted/updated record</returns>
    Task<Guid> Upsert(LncSeries item);

    /// <summary>
    /// Gets all of the records from the lnc_series table
    /// </summary>
    /// <returns>All of the records</returns>
    Task<LncSeries[]> Get();
}

internal class LncSeriesDbService(
	IOrmService orm) : Orm<LncSeries>(orm), ILncSeriesDbService
{

}
