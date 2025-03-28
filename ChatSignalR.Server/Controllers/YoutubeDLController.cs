using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ChatSignalR.Server.Service;
using ChatSignalR.Server.Dto;
namespace ChatSignalR.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YoutubeDLController(DownloadService downloadService) : ControllerBase
    {
        private readonly DownloadService _downloadService = downloadService;

        [HttpPost("start-download")]
        public async Task<IActionResult> StartDownload([FromBody] DownloadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest("URL cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(request.OutputFolder))
            {
                return BadRequest("Output folder cannot be empty.");
            }
            // The client needs to provide the connection ID as part of the query string or headers
            string? connectionId = HttpContext.Request.Query["connectionId"];
            if (string.IsNullOrEmpty(connectionId))
            {
                return BadRequest("Connection ID is required.");
            }

            // Start the download using the service
            await _downloadService.StartDownload(connectionId, request.Url, request.AudioOnly, request.OutputFolder);

            // Return a response confirming the download has started
            return Ok(new { Message = "Download started successfully." });
        }
    }
}
