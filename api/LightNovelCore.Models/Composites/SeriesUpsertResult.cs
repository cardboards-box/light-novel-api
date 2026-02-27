namespace LightNovelCore.Models.Composites;

/// <summary>
/// The results of upserting novels 
/// </summary>
/// <param name="Publishers">The new publishers</param>
/// <param name="Series">The new series</param>
/// <param name="Volumes">The new volumes</param>
/// <param name="Publications">The new publications</param>
public record class SeriesUpsertResult(
	[property: JsonPropertyName("publishers")] LncPublisher[] Publishers,
	[property: JsonPropertyName("series")] LncSeries[] Series,
	[property: JsonPropertyName("volumes")] LncVolume[] Volumes,
	[property: JsonPropertyName("publications")] LncPublication[] Publications);
