
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Models.CourseTopicModels;
using OnlineCoursePlatform.Models.PagingAndFilter;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.CourseTopics;

namespace OnlineCoursePlatform.Repositories.CourseTopicRepositories.Interfaces
{
    public interface ICourseTopicRepository
    {

        Task<PagedList<CourseTopicInfoModel>> GetAllsAsync(CourseTopicFilterParams pagingAndFilterParams);
        Task<PagedList<CourseTopicInfoModel>> GetAllsSecondFilterAsync(
            CourseTopicSecondFilterParams courseTopicSecondFilterParams);
        Task<CourseTopic> AddCourseTopicAsync(CourseTopic courseTopic);
        Task DeleteCourseTopicByIdAsync(CourseTopic courseTopic);
        Task<CourseTopic?> FindCourseTopicByIdAsync(int id);
        Task<CourseTopic> UpdateCourseTopicAsync(CourseTopic courseTopic);
    }
}