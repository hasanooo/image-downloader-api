using ImageDownloader.API.Interfaces;
using ImageDownloader.API.Models;

namespace ImageDownloader.API.Services
{
    public class ImageService : IImageService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ImageService> _logger;
        private readonly string _storageFolder;
        public ImageService(HttpClient httpClient,
                        ILogger<ImageService> logger,
                        IWebHostEnvironment env)
        {
            _httpClient = httpClient;
            _logger = logger;
            _storageFolder = Path.Combine(env.ContentRootPath, "DownloadedImages");

            Directory.CreateDirectory(_storageFolder);
        }
        public async Task<ResponseDownload> DownloadImagesAsync(
        RequestedDownload request, CancellationToken ct = default)
        {
            var urlAndNames = new Dictionary<string, string>();
            var errors = new List<string>();

            using var semaphore = new SemaphoreSlim(
                initialCount: request.MaxDownloadAtOnce,
                maxCount: request.MaxDownloadAtOnce);


            var tasks = request.ImageUrls.Select(url => DownloadOneAsync(url, semaphore, ct));

            var results = await Task.WhenAll(tasks);

            foreach (var (url, fileName, error) in results)
            {
                if (error is null)
                    urlAndNames[url] = fileName!;
                else
                    errors.Add($"{url} → {error}");
            }

            return new ResponseDownload
            {
                Success = errors.Count == 0,
                Message = errors.Count == 0
                                ? $"All {urlAndNames.Count} image(s) downloaded successfully."
                                : $"{urlAndNames.Count} succeeded, {errors.Count} failed: {string.Join(" | ", errors)}",
                UrlAndNames = urlAndNames
            };
        }
        private async Task<(string Url, string? FileName, string? Error)> DownloadOneAsync(
         string url, SemaphoreSlim semaphore, CancellationToken ct)
        {

            await semaphore.WaitAsync(ct);
            try
            {
                _logger.LogInformation("Starting download: {Url}", url);


                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();


                var extension = GetExtensionFromContentType(
                    response.Content.Headers.ContentType?.MediaType);


                var fileName = $"{Guid.NewGuid()}{extension}";
                var fullPath = Path.Combine(_storageFolder, fileName);


                await using var fileStream = new FileStream(
                    fullPath, FileMode.Create, FileAccess.Write,
                    FileShare.None, bufferSize: 81920, useAsync: true);

                await response.Content.CopyToAsync(fileStream, ct);

                _logger.LogInformation("Saved {Url} → {FileName}", url, fileName);
                return (url, fileName, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download {Url}", url);
                return (url, null, ex.Message);
            }
            finally
            {

                semaphore.Release();
            }
        }
        private static string GetExtensionFromContentType(string? mediaType) =>
        mediaType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            "image/bmp" => ".bmp",
            "image/svg+xml" => ".svg",
            _ => ".jpg"
        };
    }

}
