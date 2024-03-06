namespace OnlineCoursePlatform.DTOs.CourseDtos.Response
{
    public class CreateCourseResponseDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = null!;
        public decimal Price { get; set; } = 0;
        public string? ThumbnailUrl { get; set; }
        public string? CourseDescription { get; set; }
        public StreamingDto? StreamingDto { get; set; }
        public List<SubtitleDto>? SubtitleDtos { get; set; }
    }
}