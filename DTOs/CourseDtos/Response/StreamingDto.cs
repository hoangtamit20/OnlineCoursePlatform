using System.Text.Json.Serialization;

namespace OnlineCoursePlatform.DTOs.CourseDtos.Response
{
    public class StreamingDto
    {
        public string? Token { get; set; }
        // public string? UrlStreamHlsCsf { get; set; }
        // public string? UrlStreamHlsCmaf { get; set; }
        public string? UrlStreamDashCsf { get; set; }
        public string? UrlStreamDashCmaf { get; set; }
        public string? UrlSmoothStreaming { get; set; }
        public string? PlayReadyUrlLicenseServer { get; set; }
        public string? WidevineUrlLicenseServer { get; set; }
        [JsonIgnore]
        public string? KeyIdentifier { get; set; }
    }
}