using ImageDownloader.API.Models;

namespace ImageDownloader.API.Interfaces
{
    public interface IImageService
    {
        Task<ResponseDownload> DownloadImagesAsync(RequestedDownload request, CancellationToken ct = default);
    }
}
