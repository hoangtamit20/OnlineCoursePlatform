
using OnlineCoursePlatform.DTOs.LessonDtos;

namespace OnlineCoursePlatform.Services.LessonServices
{
    public interface ILessonService
    {
        Task<AddLessResponseDto?> AddLessonAsync(AddLessonRequestDto requestDto);
    }
}