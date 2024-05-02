using System.ComponentModel.DataAnnotations;

namespace OnlineCoursePlatform.DTOs.CourseDtos
{
    public class BaseUploadVideo
    {
        public string? UrlStreamHlsCsf { get; set; }
        [StringLength(maximumLength: 1000)]
        public string? UrlStreamHlsCmaf { get; set; }
        [StringLength(maximumLength: 1000)]
        public string? UrlStreamDashCsf { get; set; }
        [StringLength(maximumLength: 1000)]
        public string? UrlStreamDashCmaf { get; set; }
        [StringLength(maximumLength: 1000)]
        public string? UrlSmoothStreaming { get; set; }
        [StringLength(maximumLength: 500)]
        public string? PlayReadyUrlLicenseServer { get; set; }
        [StringLength(maximumLength: 500)]
        public string? WidevineUrlLicenseServer { get; set; }
        [StringLength(maximumLength: 250)]
        public string? AssetName { get; set; }
        [StringLength(maximumLength: 250)]
        public string? LocatorName { get; set; }
        [StringLength(maximumLength: 500)]
        public string? SigningTokenKey { get; set; }
        [StringLength(maximumLength: 250)]
        public string? IdentifierKey { get; set; }
    }
}