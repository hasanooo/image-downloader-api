using ImageDownloader.API.Models;

namespace ImageDownloader.API.Interfaces
{
    public interface IImageService
    {
        Task<ResponseDownload> DownloadImagesAsync(RequestedDownload request, CancellationToken ct = default);
        Task<string> GetImageAsBase64Async(string imageName, CancellationToken ct = default);
    }
}
