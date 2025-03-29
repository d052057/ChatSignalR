using Microsoft.AspNetCore.Mvc;
using ChatSignalR.Server.Service;
using ChatSignalR.Server.Dto;
using ChatSignalR.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
namespace ChatSignalR.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YoutubeDLController : ControllerBase
    {
        private readonly DownloadService _downloadService;
        private readonly IHubContext<DownloadHub> _hubContext;
        public YoutubeDLController(
            DownloadService downloadService,
            IHubContext<DownloadHub> hubContext)
        {
            _downloadService = downloadService;
            _hubContext = hubContext;
        }


        [HttpPost("start-download")]
        public async Task<IActionResult> StartDownloadAsync([FromBody] DownloadRequest request)
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
            //await _downloadService.StartDownloadAsync(connectionId, request.Url, request.AudioOnly, request.OutputFolder);

            //// Return a response confirming the download has started
            //return Ok(new { Message = "Download started successfully." });
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", "Controller Download started...");
            try
            {
                // Call the download service and pass a callback function to handle progress
                await _downloadService.StartDownloadAsync(
                    connectionId,
                    request.Url,
                    request.AudioOnly,
                    request.OutputFolder,
                    async (progress) =>
                    {
                        // Send progress updates to the specific client
                        await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", $"Progress: {progress}%");
                    });

                // Notify the client that the download is complete
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", "Download completed!");
                return Ok("Download successfully completed!");
            }
            catch (Exception ex)
            {
                // Notify the client about the error
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveError", $"Error during download: {ex.Message}");
                return StatusCode(500, "Error during download.");
            }
        }
    }
}
