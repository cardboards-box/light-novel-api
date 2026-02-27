namespace LightNovelCore.Database.Services;

using Models;

/// <summary>
/// The service for interacting with the lnc_publishers table
/// </summary>
public interface ILncPublisherDbService
{
    /// <summary>
    /// Fetches a record by its ID from the lnc_publishers table
    /// </summary>
    /// <param name="id">The ID of the record</param>
    /// <returns>The record</returns>
    Task<LncPublisher?> Fetch(Guid id);

    /// <summary>
    /// Inserts a record into the lnc_publishers table
    /// </summary>
    /// <param name="item">The item to insert</param>
    /// <returns>The ID of the inserted record</returns>
    Task<Guid> Insert(LncPublisher item);

    /// <summary>
    /// Updates a record in the lnc_publishers table
    /// </summary>
    /// <param name="item">The record to update</param>
    /// <returns>The number of records updated</returns>
    Task<int> Update(LncPublisher item);

    /// <summary>
    /// Inserts a record in the lnc_publishers table if it doesn't exist, otherwise updates it
    /// </summary>
    /// <param name="item">The item to update or insert</param>
    /// <returns>The ID of the inserted/updated record</returns>
    Task<Guid> Upsert(LncPublisher item);

    /// <summary>
    /// Gets all of the records from the lnc_publishers table
    /// </summary>
    /// <returns>All of the records</returns>
    Task<LncPublisher[]> Get();
}

internal class LncPublisherDbService(
    IOrmService orm) : Orm<LncPublisher>(orm), ILncPublisherDbService
{

}