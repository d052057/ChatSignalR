using Microsoft.AspNetCore.SignalR;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;
using ChatSignalR.Server.Dto;
using ChatSignalR.Server.Hubs;
namespace ChatSignalR.Server.Service
{
    public class DownloadService
    {
        private readonly IHubContext<DownloadHub> _hubContext;
        private readonly YoutubeDL _youtubeDL;

        public DownloadService(IHubContext<DownloadHub> hubContext)
        {
            _hubContext = hubContext;
            _youtubeDL = new YoutubeDL
            {
                YoutubeDLPath = "yt-dlp.exe",
                FFmpegPath = "ffmpeg.exe",
                // This property is not used by the raw process call below.
                OutputFolder = @"c:\medias\poster"
            };
        }

        public async Task StartDownloadAsync(string connectionId, string url, bool audioOnly, string outputFolder)
        {
            try
            {
                // Build the yt-dlp arguments with the output option (-o) using the UI supplied folder.
                string arguments = audioOnly
                    ? $"-x --audio-format mp3 -o \"{outputFolder}\\%(title)s.%(ext)s\" \"{url}\""
                    : $"-o \"{outputFolder}\\%(title)s.%(ext)s\" \"{url}\"";

                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "yt-dlp.exe",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.OutputDataReceived += async (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        await _hubContext.Clients.Client(connectionId)
                            .SendAsync("ReceiveProgress", args.Data);
                    }
                };

                process.ErrorDataReceived += async (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        await _hubContext.Clients.Client(connectionId)
                            .SendAsync("ReceiveError", args.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Wait for the process to complete
                await process.WaitForExitAsync();

                // Notify the client after the download is complete.
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("DownloadFinished", "Download complete!");
            }
            catch (Exception ex)
            {
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ReceiveError", $"An error occurred: {ex.Message}");
            }
        }
    }
}
