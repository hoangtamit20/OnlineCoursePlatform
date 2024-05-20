
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.LessonDtos;

namespace OnlineCoursePlatform.Services.LessonServices
{
    public interface ILessonService
    {
        Task<(int statusCode, BaseResponseWithData<AddLessResponseDto> result)> AddLessonAsync(AddLessonRequestDto requestDto);
        Task<(int statusCode, BaseResponseWithData<List<LessonResponseDto>> data)> 
            GetLessonsOfCourseAsync(GetLessonsOfCourseRequestDto requestDto);
        Task<(int statusCode , BaseResponseWithData<LessonDetailResponseDto> result)>
            GetLessonDetailAsync(GetLessonDetailRequestDto requestDto);
    }
}