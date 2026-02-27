using CardboardBox.Http;

namespace LightNovelCore.DataSet;

using Models;

/// <summary>
/// A service for fetching novel data from the API
/// </summary>
public interface INovelApiService
{
	/// <summary>
	/// Fetches the novels from the API
	/// </summary>
	/// <returns>The novels</returns>
	Task<Novel[]> Get();
}

internal class NovelApiService(
	IApiService _api) : INovelApiService
{
	public const string URL = "https://lnrelease.github.io/data.json";

	public Task<Novel[]> Get()
	{
		return _api.Get<NovelData>(URL).ContinueWith(t => t.Result?.Books?.ToArray() ?? []);
	}
}
