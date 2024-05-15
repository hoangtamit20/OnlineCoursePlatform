namespace OnlineCoursePlatform.DTOs.LessonDtos
{
    public class AddLessonRequestDto
    {
        public string Name { get; set; } = null!;
        public int LessonIndex { get; set; }
        public DateTime? DateRealease { get; set; }
        public bool? IsPublic { get; set; }
        public string? Description { get; set; }
        public IFormFile? ThumbnailFile { get; set; }
        public List<IFormFile>? SubtitleFiles { get; set; }
        public IFormFile VideoFile { get; set; } = null!;
        public int CourseId { get; internal set; }
    }

    public class AddLessResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int LessonIndex { get; set; }
        public string? Description { get; set; }
    }
}