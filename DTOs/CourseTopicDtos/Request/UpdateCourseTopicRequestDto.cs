namespace OnlineCoursePlatform.DTOs.CourseTopicDtos.Request
{
    public class UpdateCourseTopicRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int CourseTypeId { get; set; }
    }
}