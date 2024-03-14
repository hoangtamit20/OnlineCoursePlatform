namespace OnlineCoursePlatform.Models.CourseModels
{
    public class CourseInfoModel
    {
        public int Id { get; set; }
        public string CourseName { get; set; } = null!;
        public decimal Price { get; set; } = 0;
        public string? Thumbnail { get; set; }
        public bool? IsFree { get; set; }
        public int WeeklyViews { get; set; } = 0;
        public int MonthlyViews { get; set; } = 0;
        public string CreatorId { get; set; } = null!;
        public string CreatorName { get; set; } = null!;
        public string? CreatorPicture { get; set; }
    }
}