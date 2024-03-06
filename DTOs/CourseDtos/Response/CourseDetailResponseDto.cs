using System.Text.Json.Serialization;

namespace OnlineCoursePlatform.DTOs.CourseDtos.Response
{
    public class CourseDetailResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; } = 0;
        public string? Thumbnail { get; set; }
        public DateTime DateCreate { get; set; }
        public int WeeklyViews { get; set; }
        public int MonthlyViews { get; set; }
        public int TotalViews { get; set; }
        public int MonthlySales { get; set; }
        public int TotalSales { get; set; }
        public Publisher? Publisher { get; set; }
        public List<SubtitleDto>? CourseSubtitles { get; set; }
        public StreamingDto? CourseUrlStreaming { get; set; }
        public List<LessonCourseDetailDto>? Lessons { get; set; }

    }
}