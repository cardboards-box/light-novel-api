namespace LightNovelCore.Database;

using Services;

/// <summary>
/// The service for interacting with the database
/// </summary>
public interface IDbService
{
	/// <summary>
	/// The service for interacting with the lnc_covers table
	/// </summary>
	ILncCoverDbService Cover { get; }

	/// <summary>
	/// The service for interacting with the lnc_publications table
	/// </summary>
	ILncPublicationDbService Publication { get; }

	/// <summary>
	/// The service for interacting with the lnc_publishers table
	/// </summary>
	ILncPublisherDbService Publisher { get; }

	/// <summary>
	/// The service for interacting with the lnc_series table
	/// </summary>
	ILncSeriesDbService Series { get; }

	/// <summary>
	/// The service for interacting with the lnc_volumes table
	/// </summary>
	ILncVolumeDbService Volume { get; }

	/// <summary>
	/// The service for interacting with the lnc_novel_staging table
	/// </summary>
	ILncNovelStagingDbService Staging { get; }
}

internal class DbService(IServiceProvider _provider) : IDbService
{
	#region Lazy Loaded Service Caches
	private ILncCoverDbService? _cover;
	private ILncPublicationDbService? _publication;
	private ILncPublisherDbService? _publisher;
	private ILncSeriesDbService? _series;
	private ILncVolumeDbService? _volume;
	private ILncNovelStagingDbService? _staging;
	#endregion

	#region Service Implementations
	public ILncCoverDbService Cover => _cover ??= _provider.GetRequiredService<ILncCoverDbService>();
	public ILncPublicationDbService Publication => _publication ??= _provider.GetRequiredService<ILncPublicationDbService>();
	public ILncPublisherDbService Publisher => _publisher ??= _provider.GetRequiredService<ILncPublisherDbService>();
	public ILncSeriesDbService Series => _series ??= _provider.GetRequiredService<ILncSeriesDbService>();
	public ILncVolumeDbService Volume => _volume ??= _provider.GetRequiredService<ILncVolumeDbService>();
	public ILncNovelStagingDbService Staging => _staging ??= _provider.GetRequiredService<ILncNovelStagingDbService>();
	#endregion
}
