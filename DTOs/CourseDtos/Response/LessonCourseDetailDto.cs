namespace OnlineCoursePlatform.DTOs.CourseDtos.Response
{
    public class LessonCourseDetailDto
    {
        public int LessonId { get; set; }
        public string? LessonName { get; set; }
        public int LessonIndex { get; set; }
        public bool? IsPublic { get; set; }
        public string? LessonThumbnail { get; set; }
    }
}