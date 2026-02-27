using System.Threading.RateLimiting;

namespace LightNovelCore.Services;

using FKS = FromKeyedServicesAttribute;

/// <summary>
/// A service for caching cover images
/// </summary>
public interface ICoverCacheService
{
	/// <summary>
	/// The place where the image cache is stored
	/// </summary>
	string StoragePath { get; }

	/// <summary>
	/// Fetches the image result
	/// </summary>
	/// <param name="image">The image to fetch</param>
	/// <param name="token">The cancellation token</param>
	/// <returns>The result of the image fetch</returns>
	Task<ImageResult> Get(LncCover image, CancellationToken token);

	/// <summary>
	/// Fetches the image result
	/// </summary>
	/// <param name="id">The ID of the image to fetch</param>
	/// <param name="token">The cancellation token</param>
	/// <returns>The result of the image fetch</returns>
	Task<ImageResult> Get(Guid id, CancellationToken token);

	/// <summary>
	/// Process all of the ISBNs that are missing cover images
	/// </summary>
	/// <param name="token">The cancellation token</param>
	Task ProcessMissing(CancellationToken token);
}

internal class CoverCacheService(
	IDbService _db,
	IHttpService _http,
	IConfiguration _config,
	ICoverApiService _covers,
	ILogger<CoverCacheService> _logger,
	[FKS(CoverCacheService.LIMITER_KEY)] RateLimiter _limiter) : ICoverCacheService
{
	public const string LIMITER_KEY = nameof(CoverCacheService) + "Limiter";
	public const string EXT_DAT = HttpService.EXT_DAT;

	private int? _failures;
	private TimeSpan? _waitPeriod;

	/// <inheritdoc />
	public string StoragePath => field ??= _config["Imaging:CacheDir"]?.ForceNull() ?? "file-cache";

	/// <summary>
	/// The number of times to wait before the image gets deleted
	/// </summary>
	public int FailuresBeforeDelete => _failures ??= int.TryParse(_config["Imaging:FailuresBeforeDelete"], out var f) ? f : 4;

	/// <summary>
	/// How long to wait between requesting failed images in seconds
	/// </summary>
	public double ErrorWaitPeriodSeconds => double.TryParse(_config["Imaging:ErrorWaitPeriod"], out var sec) ? sec : 60 * 60 * 24;

	/// <summary>
	/// How long to wait between requesting failed images
	/// </summary>
	public TimeSpan ErrorWaitPeriod => _waitPeriod ??= TimeSpan.FromSeconds(ErrorWaitPeriodSeconds);

	/// <summary>
	/// Get the cacha path for the given image
	/// </summary>
	/// <param name="url">The URL to the image</param>
	/// <param name="hash">The hash of the URL</param>
	/// <returns>The path to the cached image</returns>
	public string CachePath(string url, out string hash)
	{
		var path = Path.Combine([.. StoragePath.Split(['\\', '/'])]);
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
			_logger.LogInformation("Created image cache directory >> {Path}", path);
		}

		hash = url.MD5Hash();
		return Path.Combine(StoragePath, $"{hash}.{EXT_DAT}");
	}

	/// <inheritdoc />
	public async Task<ImageResult> Get(LncCover image, CancellationToken token)
	{
		async Task<ImageResult> HandleError(string reason)
		{
			image.LastFailedAt = DateTime.UtcNow;

			if (!string.IsNullOrEmpty(image.FailedReason))
				image.FailedReason += "\n" + reason;
			else image.FailedReason = reason;

			image.FailedCount += 1;
			await _db.Cover.Update(image);

			if (image.FailedCount > FailuresBeforeDelete)
				await _db.Cover.Delete(image.Id);

			_logger.LogWarning("Image failed to load: {Id} ({Count}) >> {Url} >> {Reason}", 
				image.Id, image.FailedCount, image.CoverUrl, reason);
			return new(reason, image);
		}

		try
		{
			if (string.IsNullOrEmpty(image.CoverUrl))
				return new("No cover URL", image);

			var url = image.CoverUrl!;
			var path = CachePath(url, out var hash);
			if (File.Exists(path) &&
				!string.IsNullOrEmpty(image.FileName) &&
				!string.IsNullOrEmpty(image.MimeType))
				return new(null, image, File.OpenRead(path), true);

			if (image.LastFailedAt is not null && image.LastFailedAt.Value + ErrorWaitPeriod > DateTime.UtcNow)
			{
				var waitTime = (image.LastFailedAt.Value + ErrorWaitPeriod) - DateTime.UtcNow;
				return new($"Image is in cooldown period. Retry after {waitTime.TotalSeconds:F0} seconds", image);
			}

			using var download = await _http.Download(url, null, token);
			if (!string.IsNullOrEmpty(download.Error) || download.Stream is null)
				return await HandleError(download.Error ?? "Stream came back empty!");

			image.MimeType ??= download.MimeType;
			image.ImageSize ??= download.Length ?? download.Stream.Length;
			image.FileName ??= download.FileName;
			image.UrlHash = hash;

			using var io = File.Create(path);
			await download.Stream.CopyToAsync(io, token);
			await io.FlushAsync(token);
			await io.DisposeAsync();

			if (image.ImageWidth is null || image.ImageHeight is null)
			{
				var (width, height) = await _http.DetermineImageSize(path);
				image.ImageWidth = width ?? image.ImageWidth;
				image.ImageHeight = height ?? image.ImageHeight;
			}

			await _db.Cover.Update(image);

			return new(null, image, File.OpenRead(path), false);
		}
		catch (OperationCanceledException)
		{
			return new("Request cancelled", image);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to fetch image >> {URL}", image.CoverUrl);
			return await HandleError("Image not found - " + ex.Message);
		}
	}

	/// <inheritdoc />
	public async Task<ImageResult> Get(Guid id, CancellationToken token)
	{
		var image = await _db.Cover.Fetch(id);
		if (image is null)
			return new("Image not found", new() { Id = id });

		return await Get(image, token);
	}

	/// <summary>
	/// Handles loading the cover for the given ISBN
	/// </summary>
	/// <param name="isbn">The ISBN of the book</param>
	/// <param name="token">The cancellation token</param>
	/// <returns>Whether the cover was successfully cached</returns>
	public async ValueTask<bool> CacheCoverImage(string isbn, CancellationToken token)
	{
		if (string.IsNullOrEmpty(isbn)) return false;

		try
		{
			using var lease = await _limiter.AcquireAsync(1, token);
			if (!lease.IsAcquired)
			{
				_logger.LogWarning("Failed to acquire rate limiter lease for caching cover image >> {ISBN}", isbn);
				return false;
			}

			var response = await _covers.Get(isbn);
			await _db.Cover.Upsert(new()
			{
				Isbn = isbn,
				FailedReason = response.Error,
				CoverUrl = response.Url,
				LastFailedAt = response.Error is not null ? DateTime.UtcNow : null,
				FailedCount = response.Error is not null ? 1 : 0,
				UrlHash = response.Url?.MD5Hash()
			});
			return !string.IsNullOrEmpty(response.Url);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while loading cover for ISBN >> {ISBN}", isbn);
			return false;
		}
	}

	/// <inheritdoc />
	public async Task ProcessMissing(CancellationToken token)
	{
		var missing = await _db.Cover.MissingISBNs();
		if (missing.Length == 0)
		{
			_logger.LogInformation("No missing cover images to process!");
			return;
		}

		var opts = new ParallelOptions
		{
			CancellationToken = token,
			MaxDegreeOfParallelism = Math.Clamp(Environment.ProcessorCount, 1, 10)
		};

		int processed = 0;
		int failed = 0;
		await Parallel.ForEachAsync(missing, opts, async (i, t) =>
		{
			var success = await CacheCoverImage(i, t);
			Interlocked.Increment(ref processed);
			if (!success) Interlocked.Increment(ref failed);

			if (processed % 100 != 0) return;

			_logger.LogInformation("Processed {Processed}/{Total} ({Percentage:P2}) missing cover images... {Failed} failures so far", 
				processed, missing.Length, (double)processed / missing.Length, failed);
		});

		_logger.LogInformation("Finished processing missing cover images! Processed {Processed} images with {Failed} failures.", processed, failed);
	}
}

/// <summary>
/// Represents a resulting image from the image service
/// </summary>
/// <param name="Error">The error message if applicable</param>
/// <param name="Stream">The image stream</param>
/// <param name="FromCache">Indicates if the image was retrieved from cache or the source</param>
/// <param name="Image">The image data</param>
public record class ImageResult(
	string? Error,
	LncCover Image,
	Stream? Stream = null,
	bool FromCache = true)
{
	/// <summary>
	/// The ID of the file
	/// </summary>
	public Guid FileId => Image.Id;

	/// <summary>
	/// The name of the file
	/// </summary>
	public string? FileName => Image?.FileName;

	/// <summary>
	/// The mime-type / content-type
	/// </summary>
	public string? MimeType => Image?.MimeType;

	/// <summary>
	/// The width of the image in pixels
	/// </summary>
	public int? Width => Image?.ImageWidth;

	/// <summary>
	/// The height of the image in pixels
	/// </summary>
	public int? Height => Image?.ImageHeight;
}