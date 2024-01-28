namespace OnlineCoursePlatform.DTOs.CourseTopicDtos.Request
{
    public class CreateCourseTopicRequestDto
    {
        public string Name { get; set; } = null!;
        public int CourseTypeId { get; set; }
    }
}