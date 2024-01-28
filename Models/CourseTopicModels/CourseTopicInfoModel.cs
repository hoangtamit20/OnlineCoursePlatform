namespace OnlineCoursePlatform.Models.CourseTopicModels
{
    public class CourseTopicInfoModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int WeeklyViews { get; set; }
        public int MonthlyViews { get; set; }
        public int TotalViews { get; set; }
        public DateTime CreateDate { get; set; }
        public int CourseTypeId { get; set; }
    }
}