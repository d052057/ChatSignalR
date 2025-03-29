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
//using Microsoft.Extensions.Configuration;
namespace ChatSignalR.Server.Hubs
{
    public class DownloadHub : Microsoft.AspNetCore.SignalR.Hub
    {
        //private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownloadService _downloadService;
        //private readonly string _apiBaseUrl;
        public DownloadHub(
            //IHttpClientFactory httpClientFactory,
            DownloadService downloadService,
            IConfiguration configuration)
        {
            //_httpClientFactory = httpClientFactory;
            _downloadService = downloadService;
            //_apiBaseUrl = configuration.GetSection("ApiSettings")["BaseUrl"] ?? @"https://localhost:7132";

            //_apiBaseUrl = configuration.GetSection("ApiSettings")["BaseUrl"] ?? throw new ArgumentNullException("BaseUrl configuration is missing");
        }
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
        public async Task HubStartDownloadServiceAsync(DownloadRequest request)
        {
            await Clients.All.SendAsync("ReceiveProgress", "Starting Download From Service");
            try
            {
                string conn = GetConnectionId();
                await _downloadService.StartDownloadAsync(conn, request.Url, request.AudioOnly, request.OutputFolder);
            }
            catch (Exception ex)
            {
                await Clients.All.SendAsync("ReceiveError", $"Error during download: {ex.Message}");
            }
        }
        //public async Task HubStartDownloadControllerAsync(DownloadRequest request)
        //{
        //    await Clients.All.SendAsync("ReceiveMessage", "Starting Download From Control");

        //    var client = _httpClientFactory.CreateClient();
        //    var response = await client.PostAsJsonAsync("/api/Download", request);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var result = await response.Content.ReadAsStringAsync();
        //        await Clients.All.SendAsync("ReceiveMessage", $"Download completed: {result}");
        //    }
        //    else
        //    {
        //        await Clients.All.SendAsync("ReceiveMessage", $"Failed to download: {response.StatusCode}");
        //    }
        //}
    }
}
