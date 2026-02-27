namespace LightNovelCore.DataSet;

/// <summary>
/// A service for fetching cover images from the source APIs
/// </summary>
public interface ICoverApiService
{
	/// <summary>
	/// Gets a cover by it's ISBN
	/// </summary>
	/// <param name="isbn">The ISBN of the book</param>
	/// <returns>The cover response</returns>
	Task<CoverResponse> Get(string isbn);
}

internal class CoverApiService(
	IApiService _api,
	IConfiguration _config) : ICoverApiService
{
	public string Url => field ??= _config["Covers:Url"]?.ForceNull()?.TrimEnd('/') ?? throw new ArgumentNullException("Covers:Url");

	public async Task<CoverResponse> Get(string isbn)
	{
		var result = await _api.Get<CoverResponse, CoverResponse>($"{Url}/bookcover?isbn={isbn}");
		if (result is null) return new() { Error = "No response from cover API" };

		return (result.Success ? result.Result! : result.ErrorResult) 
			?? new() { Error = "Unknown error from cover API" };
	}
}
