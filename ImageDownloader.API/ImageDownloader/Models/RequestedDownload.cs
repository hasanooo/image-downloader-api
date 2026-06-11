namespace ImageDownloader.API.Models
{
    public class RequestedDownload
    {
        public IEnumerable<string> ImageUrls { get; set; }

        public int MaxDownloadAtOnce { get; set; } = 3;
    }
}
