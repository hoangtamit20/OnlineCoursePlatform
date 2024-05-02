using OnlineCoursePlatform.Data.Entities;

namespace OnlineCoursePlatform.Repositories.LessonRepositories
{
    public interface ILessonRepository
    {
        Task AddLessonUrlStreamingAsync(LessonUrlStreaming lessonUrlStreaming);
        Task AddRangeLessonSubtitlesAsync(List<LessonSubtitle> listItem);
        Task UpdateLessonAsync(Lesson lesson);
    }
}