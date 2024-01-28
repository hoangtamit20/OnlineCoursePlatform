using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.CourseTypeDtos.Response;
using OnlineCoursePlatform.Models.CourseTypeModels;
using OnlineCoursePlatform.Models.PagingAndFilter;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.CourseTypes;

namespace OnlineCoursePlatform.Repositories.CourseTypeRepositories
{
    public interface ICourseTypeRepository
    {
        Task<PagedList<CourseTypeInfoModel>> GetAllsAsync(CourseTypeFilterParams courseTypeFilterParams);
        Task<PagedList<CourseTypeInfoModel>> GetAllsSecondFilterAsync(CourseTypeSecondFilterParams courseTypeSecondFilterParams);
        Task<CourseTypeInfoModel?> FindCourseTypeByIdAsync(int idCourseType);
        Task DeleteCourseTypeByIdAsync(CourseType courseType);
        Task<CreateCourseTypeResponseDto> AddCourseTypeAsync(CourseType courseType);
        Task<CourseType> UpdateCourseTypeAsync(CourseType courseType);
    }
}