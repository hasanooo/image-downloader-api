using ImageDownloader.API.Interfaces;
using ImageDownloader.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ImageDownloader.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageDownloadController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly ILogger<ImageDownloadController> _logger;

        public ImageDownloadController(IImageService imageService, ILogger<ImageDownloadController> logger)
        {
            _imageService = imageService;
            _logger = logger;
        }

        [HttpPost("download")]
        public async Task<IActionResult> DownloadImages(
        [FromBody] RequestedDownload request,
        CancellationToken ct)
        {

            if (request.ImageUrls is null || !request.ImageUrls.Any())
                return BadRequest(new ResponseDownload
                {
                    Success = false,
                    Message = "ImageUrls must contain at least one URL.",
                    UrlAndNames = new Dictionary<string, string>()
                });

            if (request.MaxDownloadAtOnce <= 0)
                return BadRequest(new ResponseDownload
                {
                    Success = false,
                    Message = "MaxDownloadAtOnce must be greater than zero.",
                    UrlAndNames = new Dictionary<string, string>()
                });

            var result = await _imageService.DownloadImagesAsync(request, ct);

            var statusCode = result.Success ? 200 : 207;
            return StatusCode(statusCode, result);
        }
        [HttpGet("get-image-by-name/{image_name}")]
        public async Task<IActionResult> GetImageByName(
        [FromRoute] string image_name,
        CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(image_name))
                return BadRequest("image_name cannot be empty.");

            try
            {
                var base64 = await _imageService.GetImageAsBase64Async(image_name, ct);

                return Ok(new
                {
                    FileName = image_name,
                    Base64String = base64
                });
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { Message = ex.Message });
            }
        }


    }
}
