namespace LightNovelCore.Database.Services;

using Models;

/// <summary>
/// The service for interacting with the lnc_covers table
/// </summary>
public interface ILncCoverDbService
{
    /// <summary>
    /// Fetches a record by its ID from the lnc_covers table
    /// </summary>
    /// <param name="id">The ID of the record</param>
    /// <returns>The record</returns>
    Task<LncCover?> Fetch(Guid id);

    /// <summary>
    /// Inserts a record into the lnc_covers table
    /// </summary>
    /// <param name="item">The item to insert</param>
    /// <returns>The ID of the inserted record</returns>
    Task<Guid> Insert(LncCover item);

    /// <summary>
    /// Updates a record in the lnc_covers table
    /// </summary>
    /// <param name="item">The record to update</param>
    /// <returns>The number of records updated</returns>
    Task<int> Update(LncCover item);

    /// <summary>
    /// Inserts a record in the lnc_covers table if it doesn't exist, otherwise updates it
    /// </summary>
    /// <param name="item">The item to update or insert</param>
    /// <returns>The ID of the inserted/updated record</returns>
    Task<Guid> Upsert(LncCover item);

    /// <summary>
    /// Gets all of the records from the lnc_covers table
    /// </summary>
    /// <returns>All of the records</returns>
    Task<LncCover[]> Get();
}

internal class LncCoverDbService(
    IOrmService orm) : Orm<LncCover>(orm), ILncCoverDbService
{

}