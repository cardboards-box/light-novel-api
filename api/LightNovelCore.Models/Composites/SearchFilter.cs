namespace LightNovelCore.Models.Composites;

using Types;

/// <summary>
/// The filters for searching for publications
/// </summary>
public class SearchFilter
{
	private static readonly Random _rnd = new();

	/// <summary>
	/// The start range for the filter
	/// </summary>
	[JsonPropertyName("start")]
	public DateTime? Start { get; set; }

	/// <summary>
	/// The end range for the filter
	/// </summary>
	[JsonPropertyName("end")]
	public DateTime? End { get; set; }

	/// <summary>
	/// All of the publishers to filter by
	/// </summary>
	[JsonPropertyName("publisher")]
	public Guid[]? Publisher { get; set; }

	/// <summary>
	/// Released or not released (if null, both will be included)
	/// </summary>
	[JsonPropertyName("released")]
	public bool? Released { get; set; }

	/// <summary>
	/// The formats to filter by
	/// </summary>
	[JsonPropertyName("format")]
	public LncFormat[]? Format { get; set; }

	/// <summary>
	/// The search term to filter by
	/// </summary>
	[JsonPropertyName("search")]
	public string? Search { get; set; }

	/// <summary>
	/// All of the ISBNs to filter by
	/// </summary>
	[JsonPropertyName("isbn")]
	public string[]? Isbn { get; set; }

	/// <summary>
	/// Whether or not to filter by ascending or descending order (default: descending)
	/// </summary>
	[JsonPropertyName("asc")]
	public bool? Asc { get; set; }

	/// <summary>
	/// The maximum number of records to return
	/// </summary>
	[JsonPropertyName("size")]
	public int? Size { get; set; }

	/// <summary>
	/// The page number to return
	/// </summary>
	[JsonPropertyName("page")]
	public int? Page { get; set; }

	/// <summary>
	/// Gets a random table suffix for temp tables
	/// </summary>
	/// <returns>The order key</returns>
	public static string TableSuffix(int length = 10)
	{
		var chars = "abcdefghijklmnopqrstuvwxyz";
		return new string([.. Enumerable.Range(0, length).Select(t => chars[_rnd.Next(chars.Length)])]);
	}

	/// <summary>
	/// Builds the query and adds the appropriate parameters
	/// </summary>
	/// <param name="skipLimit">Whether or not to skip the limit</param>
	/// <param name="parameters">The parameters to be used in the query</param>
	/// <param name="size">The size of the page</param>
	/// <returns>The query string</returns>
	public string BuildQuery(bool skipLimit, out DynamicParameters parameters, out int size)
	{
		parameters = new();
		var bob = new StringBuilder();

		var suffix = TableSuffix();
		var page = skipLimit ? 1 : Math.Clamp(Page ?? 1, 1, int.MaxValue);
		size = skipLimit ? int.MaxValue : Math.Clamp(Size ?? 20, 1, 100);
		var asc = Asc ?? false;

		parameters.Add("limit", size);
		parameters.Add("offset", (page - 1) * size);

		bob.AppendLine($"""
BEGIN;
DROP TABLE IF EXISTS lnc_search_{suffix};

CREATE TEMP TABLE lnc_search_{suffix} ON COMMIT DROP AS
SELECT DISTINCT
	p.id,
	p.release_date as order_column
FROM lnc_series s
JOIN lnc_volumes v ON s.id = v.series_id
JOIN lnc_publications p ON v.id = p.volume_id
JOIN lnc_publishers r ON r.id = p.publisher_id
WHERE
	s.deleted_at IS NULL 
	AND v.deleted_at IS NULL 
	AND p.deleted_at IS NULL 
	AND r.deleted_at IS NULL
""");

		if (Start is not null)
		{
			parameters.Add("start", Start);
			bob.AppendLine("\tAND p.release_date >= @start");
		}

		if (End is not null)
		{
			parameters.Add("end", End);
			bob.AppendLine("\tAND p.release_date <= @end");
		}

		var publishers = Publisher?.Distinct().ToArray() ?? [];
		if (publishers.Length > 0)
		{
			parameters.Add("publishers", publishers);
			bob.AppendLine("\tAND p.publisher_id = ANY(@publishers)");
		}

		var formats = Format?.Select(t => (int)t).Distinct().ToArray() ?? [];
		if (formats.Length > 0)
		{
			parameters.Add("formats", formats);
			bob.AppendLine("\tAND p.format = ANY(@formats)");
		}

		var isbns = Isbn?.Distinct().Select(t => t.Replace("-", "")).ToArray() ?? [];
		if (isbns.Length > 0)
		{
			parameters.Add("isbns", isbns);
			bob.AppendLine("\tAND p.isbn = ANY(@isbns)");
		}

		if (Released is not null)
			bob.AppendLine(Released.Value
				? "\tAND p.release_date <= CURRENT_DATE"
				: "\tAND p.release_date > CURRENT_DATE");

		if (!string.IsNullOrWhiteSpace(Search))
		{
			parameters.Add("search", $"{Search.Trim()}");
			bob.AppendLine(@"	AND (
		v.fts @@ phraseto_tsquery('english', @search) OR 
		s.fts @@ phraseto_tsquery('english', @search)
	)");
		}

		bob.AppendLine($"""
;
SELECT COUNT(*) FROM lnc_search_{suffix};

CREATE TEMP TABLE lnc_search_results_{suffix} ON COMMIT DROP AS
SELECT id, order_column
FROM lnc_search_{suffix}
ORDER BY order_column {(asc ? "ASC" : "DESC")}
LIMIT @limit OFFSET @offset;

DROP TABLE lnc_search_{suffix};

SELECT DISTINCT a.*
FROM lnc_search_results_{suffix} r
JOIN lnc_publications p ON r.id = p.id
JOIN lnc_publishers a ON p.publisher_id = a.id
WHERE 
	a.deleted_at IS NULL AND
	p.deleted_at IS NULL;

SELECT DISTINCT s.*
FROM lnc_search_results_{suffix} r
JOIN lnc_publications p ON r.id = p.id
JOIN lnc_volumes v ON p.volume_id = v.id
JOIN lnc_series s ON v.series_id = s.id
WHERE 
	s.deleted_at IS NULL AND
	v.deleted_at IS NULL AND
	p.deleted_at IS NULL;
			
SELECT DISTINCT v.*
FROM lnc_search_results_{suffix} r
JOIN lnc_publications p ON r.id = p.id
JOIN lnc_volumes v ON p.volume_id = v.id
WHERE 
	v.deleted_at IS NULL AND
	p.deleted_at IS NULL;
			
SELECT DISTINCT p.*, r.order_column
FROM lnc_search_results_{suffix} r
JOIN lnc_publications p ON r.id = p.id
WHERE 
	p.deleted_at IS NULL
ORDER BY r.order_column {(asc ? "ASC" : "DESC")};

DROP TABLE lnc_search_results_{suffix};
COMMIT;
""");

		return bob.ToString();
	}
}
