namespace LightNovelCore.Api.Controllers;

/// <summary>
/// Controllers for meta data
/// </summary>
public class MetaController(
	IDbService _db,
	ILogger<MetaController> logger) : BaseController(logger)
{
	/// <summary>
	/// Fetches all of the publishers in the system
	/// </summary>
	/// <returns>The publishers</returns>
	[HttpGet, Route("meta/publishers")]
	[ProducesArray<LncPublisher>]
	public Task<IActionResult> Publishers() => Box(async () =>
	{
		var publishers = await _db.Publisher.Get();
		return Boxed.Ok(publishers);
	});

	/// <summary>
	/// The metadata for the <see cref="LncFormat"/> enum
	/// </summary>
	/// <returns>The enum descriptions</returns>
	[HttpGet, Route("meta/formats")]
	[ProducesArray<EnumDescription>]
	public Task<IActionResult> Formats() => Box(() =>
	{
		var values = LncFormat.Digital.Describe(false, false);
		return Boxed.Ok(values);
	});
}
