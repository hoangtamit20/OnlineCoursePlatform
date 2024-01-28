using Mapster;
using Microsoft.EntityFrameworkCore;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Models.CourseTopicModels;
using OnlineCoursePlatform.Models.PagingAndFilter;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.CourseTopics;
using OnlineCoursePlatform.Repositories.CourseTopicRepositories.Interfaces;

namespace OnlineCoursePlatform.Repositories.CourseTopicRepositories.Implementations
{
    public class CourseTopicRepository : ICourseTopicRepository
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;

        public CourseTopicRepository(OnlineCoursePlatformDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedList<CourseTopicInfoModel>> GetAllsAsync(CourseTopicFilterParams pagingAndFilterParams)
        {
            // Step 1 : Filters
            var collection = _dbContext.CourseTopics as IQueryable<CourseTopic>;

            if (!string.IsNullOrWhiteSpace(pagingAndFilterParams.Name))
            {
                var words = pagingAndFilterParams.Name.Trim().ToLowerInvariant().Split(' ');
                collection = collection.Where(c => words.Any(word => c.Name.Contains(word)));
            }

            // Step 2: Paging

            var pagedList = await PagedList<CourseTopic>.ToPagedListAsync(
                source: collection,
                pageNumber: pagingAndFilterParams.PageNumber,
                pageSize: pagingAndFilterParams.PageSize);

            // Step 3: Select
            var result = pagedList.Select(cto => cto.Adapt<CourseTopicInfoModel>());

            return new PagedList<CourseTopicInfoModel>(
                items: result.ToList(),
                count: pagedList.TotalCount,
                pageNumber: pagedList.CurrentPage,
                pageSize: pagedList.PageSize);
        }

        public async Task<PagedList<CourseTopicInfoModel>> GetAllsSecondFilterAsync(
            CourseTopicSecondFilterParams courseTopicSecondFilterParams)
        {
            // Step 1 : Filters
            var collection = _dbContext.CourseTypes as IQueryable<CourseType>;

            if (!string.IsNullOrWhiteSpace(courseTopicSecondFilterParams.FirstFilter))
            {
                var words = courseTopicSecondFilterParams.FirstFilter.Trim().ToLowerInvariant().Split(' ');
                collection = collection.Where(c => words.Any(word => c.Name.Contains(word)));
            }

            // Additional filter
            if (!string.IsNullOrWhiteSpace(courseTopicSecondFilterParams.SecondFilter))
            {
                var additionalWords = courseTopicSecondFilterParams.SecondFilter.Trim().ToLowerInvariant().Split(' ');
                collection = collection.Where(c => additionalWords.Any(word => c.Name.Contains(word)));
            }

            // Step 2: Paging
            var pagedList = await PagedList<CourseType>.ToPagedListAsync(
                source: collection,
                pageNumber: courseTopicSecondFilterParams.PageNumber,
                pageSize: courseTopicSecondFilterParams.PageSize);

            // Step 3: Select
            var result = pagedList.Select(ct => ct.Adapt<CourseTopicInfoModel>());

            return new PagedList<CourseTopicInfoModel>(
                items: result.ToList(),
                count: pagedList.TotalCount,
                pageNumber: pagedList.CurrentPage,
                pageSize: pagedList.PageSize);
        }


        public async Task<CourseTopic> AddCourseTopicAsync(CourseTopic courseTopic)
        {
            try
            {
                _dbContext.CourseTopics.Add(entity: courseTopic);
                await _dbContext.SaveChangesAsync();
                return courseTopic;
            }
            catch (Exception ex)
            {
                throw new Exception($"Trace : {ex.Message}");
            }
        }

        public async Task DeleteCourseTopicByIdAsync(CourseTopic courseTopic)
        {
            try
            {
                // Ensure the context is not tracking any other entity with the same ID
                var local = _dbContext.Set<CourseTopic>()
                    .Local
                    .FirstOrDefault(entry => entry.Id.Equals(courseTopic.Id));
                // If course topic has been referenced
                var courseTopicWasReferenced = await _dbContext.Courses.FirstOrDefaultAsync(ct => ct.CourseTopicId == courseTopic.Id);

                if (courseTopicWasReferenced is null)
                {
                    // If the entity is found in the local cache, then detach it
                    if (local != null)
                    {
                        _dbContext.Entry(local).State = EntityState.Detached;
                    }
                }
                _dbContext.CourseTopics.Remove(courseTopic);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<CourseTopic?> FindCourseTopicByIdAsync(int id)
        => await _dbContext.CourseTopics.FindAsync(id);

        public async Task<CourseTopic> UpdateCourseTopicAsync(CourseTopic courseTopic)
        {
            _dbContext.CourseTopics.Update(courseTopic);
            await _dbContext.SaveChangesAsync();
            return courseTopic;
        }
    }
}