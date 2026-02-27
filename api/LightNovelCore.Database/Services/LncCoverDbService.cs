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
	/// Soft deletes the record
	/// </summary>
	/// <param name="id">The ID of the record to soft delete</param>
	Task<int> Delete(Guid id);

	/// <summary>
	/// Gets all of the records from the lnc_covers table
	/// </summary>
	/// <returns>All of the records</returns>
	Task<LncCover[]> Get();

    /// <summary>
    /// Gets all the ISBNs that are missing lnc_cover records
    /// </summary>
    /// <returns>The ISBNs that are missing lnc_cover records</returns>
    Task<string[]> MissingISBNs();
}

internal class LncCoverDbService(
	IOrmService orm) : Orm<LncCover>(orm), ILncCoverDbService
{
	public Task<string[]> MissingISBNs()
	{
        const string QUERY = @"
SELECT DISTINCT p.isbn
FROM lnc_publications p
LEFT JOIN lnc_covers c ON c.isbn = p.isbn
WHERE
    p.isbn IS NOT NULL AND
    c.id IS NULL;";
        return _sql.Get<string>(QUERY);
	}

	public override Task<int> Delete(Guid id)
	{
		return Execute("UPDATE lnc_covers SET deleted_at = CURRENT_TIMESTAMP WHERE id = :id", new { id });
	}
}