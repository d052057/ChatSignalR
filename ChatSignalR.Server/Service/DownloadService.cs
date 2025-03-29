using Microsoft.AspNetCore.SignalR;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;
using ChatSignalR.Server.Dto;
using ChatSignalR.Server.Hubs;
using System.Text.RegularExpressions;
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

        public async Task StartDownloadAsync(string connectionId, string url, bool audioOnly, string outputFolder, Func<int, Task> onProgress)
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
                    //if (!string.IsNullOrEmpty(args.Data))
                    //{
                    //    await _hubContext.Clients.Client(connectionId)
                    //        .SendAsync("ReceiveProgress", args.Data);
                    //}
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        // Extract progress (if applicable) from the output
                        int progress = ParseProgress(args.Data);

                        // Send progress to SignalR client
                        await _hubContext.Clients.Client(connectionId)
                            .SendAsync("ReceiveProgress", args.Data);

                        // Call the onProgress callback if it's provided
                        if (onProgress != null)
                        {
                            await onProgress(progress);
                        }
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
        private int ParseProgress(string output)
        {
            // Example parsing logic (adapt to yt-dlp output format)
            // For instance, yt-dlp might output something like "Downloading: 50%"
            var match = Regex.Match(output, @"(\d+)%");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

    }
}
