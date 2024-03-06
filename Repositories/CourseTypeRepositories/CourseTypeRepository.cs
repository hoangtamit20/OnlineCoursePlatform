using Mapster;
using Microsoft.EntityFrameworkCore;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.CourseTypeDtos.Response;
using OnlineCoursePlatform.Models.CourseTypeModels;
using OnlineCoursePlatform.Models.PagingAndFilter;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.CourseTypes;

namespace OnlineCoursePlatform.Repositories.CourseTypeRepositories
{
    public class CourseTypeRepository : ICourseTypeRepository
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly ILogger<CourseTypeRepository> _logger;

        public CourseTypeRepository(
            OnlineCoursePlatformDbContext dbContext,
            ILogger<CourseTypeRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<CreateCourseTypeResponseDto> AddCourseTypeAsync(CourseType courseType)
        {
            try{
                _dbContext.CourseTypes.Add(entity: courseType);
                await _dbContext.SaveChangesAsync();
                return courseType.Adapt<CreateCourseTypeResponseDto>();
            }catch(Exception ex)
            {
                throw new Exception($"Trace : {ex.Message}");
            }
        }

        public async Task<CourseType?> FindCourseTypeEntityByIdAsync(int idCourseType)
        => await _dbContext.CourseTypes.FindAsync(idCourseType);

        public async Task<CourseTypeInfoModel?> FindCourseTypeByIdAsync(int idCourseType)
        => (await _dbContext.CourseTypes.FindAsync(idCourseType)).Adapt<CourseTypeInfoModel>();

        public async Task DeleteCourseTypeByIdAsync(CourseType courseType)
        {
            try
            {
                // Ensure the context is not tracking any other entity with the same ID
                var local = _dbContext.Set<CourseType>()
                    .Local
                    .FirstOrDefault(entry => entry.Id.Equals(courseType.Id));

                var courseTypeWasReferenced = await _dbContext.CourseTopics.FirstOrDefaultAsync(cp => cp.CourseTypeId == courseType.Id);

                if (courseTypeWasReferenced is null)
                {
                    // If the entity is found in the local cache, then detach it
                    if (local != null)
                    {
                        _dbContext.Entry(local).State = EntityState.Detached;
                    }
                }
                _dbContext.CourseTypes.Remove(courseType);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<PagedList<CourseTypeInfoModel>> GetAllsAsync(CourseTypeFilterParams pagingAndFilterParams)
        {
            // Step 1 : Filters
            var collection = _dbContext.CourseTypes as IQueryable<CourseType>;

            if (!string.IsNullOrWhiteSpace(pagingAndFilterParams.Name))
            {
                var words = pagingAndFilterParams.Name.Trim().ToLowerInvariant().Split(' ');
                collection = collection.Where(c => words.Any(word => c.Name.Contains(word)));
            }

            // Step 2: Paging

            var pagedList = await PagedList<CourseType>.ToPagedListAsync(
                source: collection,
                pageNumber: pagingAndFilterParams.PageNumber,
                pageSize: pagingAndFilterParams.PageSize);

            // Step 3: Select
            var result = pagedList.Select(ct => ct.Adapt<CourseTypeInfoModel>());

            return new PagedList<CourseTypeInfoModel>(
                items: result.ToList(),
                count: pagedList.TotalCount,
                pageNumber: pagedList.CurrentPage,
                pageSize: pagedList.PageSize);
        }

        public async Task<PagedList<CourseTypeInfoModel>> GetAllsSecondFilterAsync(
            CourseTypeSecondFilterParams courseTypeSecondFilterParams)
        {
            // Step 1 : Filters
            var collection = _dbContext.CourseTypes as IQueryable<CourseType>;

            if (!string.IsNullOrWhiteSpace(courseTypeSecondFilterParams.FirstFilter))
            {
                var words = courseTypeSecondFilterParams.FirstFilter.Trim().ToLowerInvariant().Split(' ');
                collection = collection.Where(c => words.Any(word => c.Name.Contains(word)));
            }

            // Additional filter
            if (!string.IsNullOrWhiteSpace(courseTypeSecondFilterParams.SecondFilter))
            {
                var additionalWords = courseTypeSecondFilterParams.SecondFilter.Trim().ToLowerInvariant().Split(' ');
                collection = collection.Where(c => additionalWords.Any(word => c.Name.Contains(word)));
            }

            // Step 2: Paging
            var pagedList = await PagedList<CourseType>.ToPagedListAsync(
                source: collection,
                pageNumber: courseTypeSecondFilterParams.PageNumber,
                pageSize: courseTypeSecondFilterParams.PageSize);

            // Step 3: Select
            var result = pagedList.Select(ct => ct.Adapt<CourseTypeInfoModel>());

            return new PagedList<CourseTypeInfoModel>(
                items: result.ToList(),
                count: pagedList.TotalCount,
                pageNumber: pagedList.CurrentPage,
                pageSize: pagedList.PageSize);
        }

        public async Task<CourseType> UpdateCourseTypeAsync(CourseType courseType)
        {
            _dbContext.CourseTypes.Update(courseType);
            await _dbContext.SaveChangesAsync();
            return courseType;
        }
    }
}