namespace OnlineCoursePlatform.Models.AzureMediaServices
{
    public class PathStreamingModel
    {
        public string? HlsStandardUrl { get; set; }
        public string? HlsCmafUrl { get; set; }
        public string? DashStandardUrl { get; set; }
        public string? DashCmafUrl { get; set; }
        public string? SmoothStreamingUrl { get; set; }
        public string? PlayReadyUrlLicenseServer { get; set; }
        public string? WidevineUrlLicenseServer { get; set; }
        public string? Token { get; set; }

    }
}