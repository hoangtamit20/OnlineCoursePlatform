using Microsoft.EntityFrameworkCore.Storage;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.CourseDtos.Response;
using OnlineCoursePlatform.Models.CourseModels;
using OnlineCoursePlatform.Models.PagingAndFilter;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.Course;

namespace OnlineCoursePlatform.Repositories.CourseRepositories
{
    public interface ICourseRepository
    {
        Task<PagedList<CourseInfoModel>> GetAllsAsync(
            CourseFilterParams pagingAndFilterParams);
        Task<PagedList<CourseInfoModel>> GetAllsSecondFilterAsync(
            CourseSecondFilterParams courseSecondFilterParams);
        Task<CourseDetailResponseDto?> GetCourseDetailtAsync(int courseId, string? userId, string? ipAddress);
        Task<bool> CheckCourseExistAsync(int courseId);
        Task DeleteRangeCourseSubtitlesAsync(List<CourseSubtitle> courseSubtitles);
        Task<IDbContextTransaction> CreateTransactionAsync();
        Task<AppUser?> FindUserByIdAsync(string userId);
        Task<Course> CreateCourseAsync(Course course);
        Task UpdateCourseAsync(Course course);
        Task AddRangeCourseSubtitlesAsync(List<CourseSubtitle> listItem);
        Task AddCourseUrlStreamingAsync(CourseUrlStreaming courseUrlStreaming);
        Task<Course?> FindCourseByIdAsync(int courseId);
    }
}