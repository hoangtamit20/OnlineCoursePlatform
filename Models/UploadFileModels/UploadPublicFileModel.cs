using OnlineCoursePlatform.Models.SubtitleModels;

namespace OnlineCoursePlatform.Models.UploadFileModels
{
    public class UploadPublicFileModel
    {
        public string ContainerName { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public DetectSubtitleModel? DetectSubtitleModel { get; set; }
    }
}