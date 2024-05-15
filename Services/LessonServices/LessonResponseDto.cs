namespace OnlineCoursePlatform.Services.LessonServices
{
    public class LessonResponseDto
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; } = null!;
        public int LessonIndex { get; internal set; }
        public string? Thumbnail { get; internal set; }
        public bool? IsPublic { get; internal set; }
    }

    public class LessonDetailResponseDto
    {
        public List<LessonSubtitleProperty> LessonSubtitleProperties { get; set; } = null!;
        public LessonStreamingProperty LessonStreamingProperty { get; set; } = null!;
    }

    public class LessonSubtitleProperty
    {
        public string Language { get; set; } = null!;
        public string SubtitleUrl { get; set; } = null!;
    }

    public class LessonStreamingProperty
    {
        public string Token { get; set; } = null!;
        public string? PlayReadyUrlLicenseServer { get; set; }
        public string? WidevineUrlLicenseServer { get; set; }
        public string? UrlStreamHlsCsf { get; set; }
        public string? UrlStreamHlsCmaf { get; set; }
        public string? UrlStreamDashCsf { get; set; }
        public string? UrlStreamDashCmaf { get; set; }
        public string? UrlSmoothStreaming { get; set; }
    }
}