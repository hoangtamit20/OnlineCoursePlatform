namespace OnlineCoursePlatform.Models.PagingAndFilter.Filter.Course
{
    public class CourseFilterProperties
    {
        public List<int>? CourseTopicIds { get; set; }
        public bool? IsFree { get; set; }
        public decimal? FromPrice { get; set; }
        public decimal? ToPrice { get; set; }
    }
}