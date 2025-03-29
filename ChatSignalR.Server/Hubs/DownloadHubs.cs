using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;
using ChatSignalR.Server.Dto;
using ChatSignalR.Server.Hubs;
using System.Threading.Tasks;
using ChatSignalR.Server.Service;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
namespace ChatSignalR.Server.Hubs
{
    public class DownloadHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownloadService _downloadService;
        private readonly string _apiBaseUrl;
        public DownloadHub(
            IHttpClientFactory httpClientFactory,
            DownloadService downloadService,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _downloadService = downloadService;
            //_apiBaseUrl = configuration.GetSection("ApiSettings")["BaseUrl"] ?? @"https://localhost:7132";

            _apiBaseUrl = configuration.GetSection("ApiSettings")["BaseUrl"] ?? throw new ArgumentNullException("BaseUrl configuration is missing");
        }
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
        public async Task HubStartDownloadServiceAsync(DownloadRequest request)
        {
            string conn = GetConnectionId();
            // Validate the request and connection ID
            if (string.IsNullOrEmpty(conn))
            {
                await Clients.All.SendAsync("ReceiveError", "Unable to determine connection ID.");
                return;
            }

            if (request == null || string.IsNullOrEmpty(request.Url) || string.IsNullOrEmpty(request.OutputFolder))
            {
                await Clients.Client(conn).SendAsync("ReceiveError", "Invalid download request.");
                return;
            }
            await Clients.Client(conn).SendAsync("ReceiveProgress", "Starting Download From Service");
            try
            {
                
                await _downloadService.StartDownloadAsync(
                    conn, 
                    request.Url, 
                    request.AudioOnly, 
                    request.OutputFolder,
                    async (progress) =>
                    {
                        // Send progress updates to the specific client
                        //await Clients.Client(conn).SendAsync("ReceiveProgress", $"Progress: {progress}%");
                        await Clients.Client(conn).SendAsync("ReceiveProgress", $"Downloading... {progress}% completed.");
                    }
                    );
                await Clients.Client(conn).SendAsync("DownloadFinished", $"Download complete! Files saved to {request.OutputFolder}.");
            }
            catch (UriFormatException)
            {
                await Clients.Client(conn).SendAsync("ReceiveError", "The URL format is invalid.");
            }
            catch (IOException)
            {
                await Clients.Client(conn).SendAsync("ReceiveError", "An error occurred while accessing the file system.");
            }
            catch (Exception ex)
            {
                await Clients.Client(conn).SendAsync("ReceiveError", $"Error during download: {ex.Message}");
            }
        }
        public async Task HubStartDownloadControllerAsync(string connectionId, DownloadRequest request)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveProgress", "Starting Download From Controller");

            var client = _httpClientFactory.CreateClient();
            var url = $"{_apiBaseUrl}/api/YoutubeDL/start-download?connectionId=${connectionId}";
            var response = await client.PostAsJsonAsync(url, request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                await Clients.Client(connectionId).SendAsync("ReceiveProgress", $"Download completed: {result}");
            }
            else
            {
                await Clients.Client(connectionId).SendAsync("ReceiveError", $"Failed to download: {response.StatusCode}");
            }
        }
    }
}
