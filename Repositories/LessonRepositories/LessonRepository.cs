using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;

namespace OnlineCoursePlatform.Repositories.LessonRepositories
{
    public class LessonRepository : ILessonRepository
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly ILogger<LessonRepository> _logger;

        public LessonRepository(
            OnlineCoursePlatformDbContext dbContext,
            ILogger<LessonRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public Task AddLessonUrlStreamingAsync(LessonUrlStreaming lessonUrlStreaming)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeLessonSubtitlesAsync(List<LessonSubtitle> listItem)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateCourseAsync(Lesson lesson)
        {
            _dbContext.Lessons.Update(entity: lesson);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occured while update lesson. {ex.Message}");
                throw new Exception($"An error occured while update lesson");
            }
        }

        public Task UpdateLessonAsync(Lesson lesson)
        {
            throw new NotImplementedException();
        }
    }
}