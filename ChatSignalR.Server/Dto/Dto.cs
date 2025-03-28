namespace ChatSignalR.Server.Dto
{
    public class DownloadRequest
    {
        public required string Url { get; set; } // The URL of the video to download
        public bool AudioOnly { get; set; } // True if the request is for audio-only downloads
        public required string OutputFolder { get; set; }
    }
}
