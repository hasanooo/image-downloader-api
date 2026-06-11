namespace ImageDownloader.API.Models
{
    public class ResponseDownload
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public IDictionary<string, string> UrlAndNames { get; set; } 
    }
}
