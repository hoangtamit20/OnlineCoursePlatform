using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.CourseDtos.Response;
using OnlineCoursePlatform.Models.CourseModels;
using OnlineCoursePlatform.Models.CourseTypeModels;
using OnlineCoursePlatform.Models.PagingAndFilter;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.Course;
using OnlineCoursePlatform.Repositories.AzureRepositories.BlobStorageRepositories;

namespace OnlineCoursePlatform.Repositories.CourseRepositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<CourseRepository> _logger;
        private readonly IBlobStorageRepository _blobStorageRepository;

        public CourseRepository(
            OnlineCoursePlatformDbContext dbContext,
            UserManager<AppUser> userManager,
            ILogger<CourseRepository> logger,
            IHttpContextAccessor httpContextAccessor,
            IBlobStorageRepository blobStorageRepository)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = logger;
            _blobStorageRepository = blobStorageRepository;
        }

        public async Task<PagedList<CourseInfoModel>> GetAllsAsync(CourseFilterParams pagingAndFilterParams)
        {
            var collection = InitCollection();
            collection = FilterByName(
                collection: collection,
                query: pagingAndFilterParams.Query);
            collection = FilterByProperties(
                collection: collection,
                filter: pagingAndFilterParams.CourseFilterProperties);
            collection = RemovePrivateCourses(collection);
            var pagedList = await ProcessPagingAsync(
                collection: collection,
                pageNumber: pagingAndFilterParams.PageNumber,
                pageSize: pagingAndFilterParams.PageSize);
            var result = SelectCourseInfo(pagedList);
            return CreatePagedList(result, pagedList);
        }

        public async Task<PagedList<CourseInfoModel>> GetAllsSecondFilterAsync(
            CourseSecondFilterParams courseSecondFilterParams)
        {
            // Step 1 : Filters
            var collection = InitCollection();

            collection = FilterByName(
                collection: collection,
                query: courseSecondFilterParams.Query);

            collection = FilterByProperties(
                collection: collection,
                filter: courseSecondFilterParams.CourseFilterProperties);
            collection = RemovePrivateCourses(collection);

            // Additional filter
            collection = FilterByName(
                collection: collection,
                query: courseSecondFilterParams.SecondQuery);

            // Step 2: Paging
            var pagedList = await ProcessPagingAsync(
                collection: collection,
                pageNumber: courseSecondFilterParams.PageNumber,
                pageSize: courseSecondFilterParams.PageSize);

            // Step 3: Select
            var result = SelectCourseInfo(pagedList);

            return CreatePagedList(result, pagedList);
        }

        // public async Task<List<CourseTypeInfoModel>> GetUserCoursesRecommendation(string? userId, string? ipAddress)
        // {
            
        // }

        private IQueryable<Course> InitCollection()
            => _dbContext.Courses.Include(c => c.User).AsQueryable();

        private IQueryable<Course> FilterByName(IQueryable<Course> collection, string? query)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                var words = query.Trim().ToLowerInvariant().Split(' ');

                collection = collection.Select(c => new
                {
                    Course = c,
                    MatchCount = words.Count(word => EF.Functions.Like(c.Name.ToLower(), "%" + word + "%"))
                                + words.Count(word => EF.Functions.Like(c.User.Name.ToLower(), "%" + word + "%"))
                })
                    .Where(x => x.MatchCount > 0)
                    .OrderByDescending(x => x.MatchCount)
                    .Select(x => x.Course);
            }

            return collection;
        }

        private IQueryable<Course> FilterByProperties(IQueryable<Course> collection, CourseFilterProperties? filter)
        {
            if (filter is not null)
            {
                if (filter.CourseTopicIds is not null)
                {
                    collection = collection.Where(c => filter.CourseTopicIds.Contains(c.CourseTopicId));
                }

                if (filter.IsFree is not null)
                {
                    collection = collection.Where(c => c.IsFree == filter.IsFree.Value);
                }

                if (filter.FromPrice is not null)
                {
                    collection = collection.Where(c => c.Price >= filter.FromPrice);
                }

                if (filter.ToPrice is not null)
                {
                    collection = collection.Where(c => c.Price <= filter.ToPrice);
                }
            }

            return collection;
        }

        private IQueryable<Course> RemovePrivateCourses(IQueryable<Course> collection)
           => collection.Where(c => c.IsPublic == true);

        private async Task<PagedList<Course>> ProcessPagingAsync(
            IQueryable<Course> collection, int pageNumber, int pageSize)
        {
            return await PagedList<Course>.ToPagedListAsync(
                source: collection,
                pageNumber: pageNumber,
                pageSize: pageSize);
        }

        private IQueryable<CourseInfoModel> SelectCourseInfo(PagedList<Course> pagedList)
        {
            return pagedList.Select(c => new CourseInfoModel()
            {
                Id = c.Id,
                CourseName = c.Name,
                Price = c.Price,
                Thumbnail = c.Thumbnail,
                IsFree = c.IsFree,
                WeeklyViews = c.WeeklyViews,
                MonthlyViews = c.MonthlyViews,
                CreatorId = c.UserId,
                CreatorName = c.User.Name,
                CreatorPicture = c.User.Picture
            }).AsQueryable();
        }

        private PagedList<CourseInfoModel> CreatePagedList(
            IQueryable<CourseInfoModel> items, PagedList<Course> pagedList)
        {
            return new PagedList<CourseInfoModel>(
                items: items.ToList(),
                count: pagedList.TotalCount,
                pageNumber: pagedList.CurrentPage,
                pageSize: pagedList.PageSize);
        }

        public async Task<AppUser?> FindUserByIdAsync(string userId)
            => await _userManager.FindByIdAsync(userId: userId);

        public async Task<Course> CreateCourseAsync(Course course)
        {
            _dbContext.Courses.Add(entity: course);
            try
            {
                await _dbContext.SaveChangesAsync();
                return course;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Create course error : {ex.Message}");
                throw new Exception($"An error occured while create course.");
            }
        }

        public async Task UpdateCourseAsync(Course course)
        {
            _dbContext.Courses.Update(entity: course);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occured while update course. {ex.Message}");
                throw new Exception($"An error occured while update course");
            }
        }

        public async Task AddRangeCourseSubtitlesAsync(List<CourseSubtitle> listItem)
        {
            _dbContext.CourseSubtitles.AddRange(entities: listItem);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Add subtitle for course error : {ex.Message}");
                throw new Exception($"An error occured while create subtitle for course");
            }
        }

        public async Task<IDbContextTransaction> CreateTransactionAsync()
            => await _dbContext.Database.BeginTransactionAsync();

        public async Task AddCourseUrlStreamingAsync(CourseUrlSteaming courseUrlStreaming)
        {
            _dbContext.CourseUrlSteamings.Add(entity: courseUrlStreaming);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error while create CourseStreamingUrls. {ex.Message}");
                throw new Exception($"An error while create CourseStreamingUrls.");
            }
        }

        public async Task<CourseDetailResponseDto?> GetCourseDetailtAsync(int courseId, string? userId, string? ipAddress)
        {
            var course = await _dbContext.Courses.FindAsync(courseId);
            var courseTopic = await _dbContext.CourseTopics.FindAsync(course?.CourseTopicId);
            var userCourseInteraction = await _dbContext.UserCourseInteractions
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == courseId);
            var courseIpAddressInteraction = await _dbContext.UserCourseInteractions
                .FirstOrDefaultAsync(uc => uc.CourseId == courseId && uc.IpAddress == ipAddress);

            if (course is null || courseTopic is null)
                return null;

            // Update views for course and course topic
            course.WeeklyViews++;
            course.MonthlyViews++;
            course.TotalViews++;
            courseTopic.WeeklyViews++;
            courseTopic.MonthlyViews++;
            courseTopic.TotalViews++;

            // Update or insert user course interaction
            // If course and user is exists in interaction => update
            if (userCourseInteraction is not null)
            {
                userCourseInteraction.ViewScore++;
            }
            // If course and user is not exists interation => insert
            else if (userId is not null)
            {
                _dbContext.UserCourseInteractions.Add(new UserCourseInteraction
                {
                    CourseId = courseId,
                    UserId = userId,
                    ViewScore = 1,
                    IpAddress = ipAddress
                });
            }
            // If user not autho, course and ipaddress is exists => update follow ipaddress
            else if (courseIpAddressInteraction is not null)
            {
                courseIpAddressInteraction.ViewScore++;
            }
            // If user not autho, course and ipaddress is not exists => insert follow ipaddress
            else
            {
                _dbContext.UserCourseInteractions.Add(new UserCourseInteraction
                {
                    CourseId = courseId,
                    ViewScore = 1,
                    IpAddress = ipAddress
                });
            }

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while creating or updating viewscore for course and user course interaction. \n{ex.Message}");
                throw new Exception("An error occurred while creating or updating viewscore for course and user course interaction");
            }

            var courseSelected = await _dbContext.Courses
                .Where(course => course.Id == courseId)
                .Select(course => new CourseDetailResponseDto
                {
                    Id = course.Id,
                    Name = course.Name,
                    Price = course.Price,
                    Thumbnail = course.Thumbnail,
                    DateCreate = course.CreateDate,
                    WeeklyViews = course.WeeklyViews,
                    MonthlyViews = course.MonthlyViews,
                    TotalViews = course.TotalViews,
                    MonthlySales = course.MonthlySales,
                    TotalSales = course.TotalSales,
                    Publisher = new Publisher
                    {
                        CreatorName = course.User.Name,
                        CreatorPicture = course.User.Picture
                    },
                    CourseSubtitles = course.CourseSubtitles.Select(cs => new SubtitleDto
                    {
                        CourseSubtitleId = cs.Id,
                        Language = cs.Language,
                        UrlSubtitle = cs.UrlSubtitle
                    }).ToList(),
                    CourseUrlStreaming = course.CourseUrlSteamings.Select(cs => new StreamingDto
                    {
                        UrlStreamDashCsf = cs.UrlStreamDashCsf,
                        UrlStreamDashCmaf = cs.UrlStreamDashCmaf,
                        UrlSmoothStreaming = cs.UrlSmoothStreaming,
                        PlayReadyUrlLicenseServer = cs.PlayReadyUrlLicenseServer,
                        WidevineUrlLicenseServer = cs.WidevineUrlLicenseServer,
                        KeyIdentifier = cs.IdentifierKey,
                        Token = ""
                    }).FirstOrDefault(),
                    Lessons = course.Lessons.Select(l => new LessonCourseDetailDto
                    {
                        LessonId = l.Id,
                        LessonName = l.Name,
                        LessonIndex = l.LessonIndex,
                        IsPublic = l.IsPublic,
                        LessonThumbnail = l.Thumbnail
                    }).ToList()
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return courseSelected;
        }


        public async Task<bool> CheckCourseExistAsync(int courseId)
            => (await _dbContext.Courses.FindAsync(courseId)) is not null;
        
        public async Task DeleteRangeCourseSubtitlesAsync(List<CourseSubtitle> courseSubtitles)
        {
            // await _blobStorageRepository.
            try
            {
                _dbContext.CourseSubtitles.RemoveRange(entities: courseSubtitles);
                await _dbContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError($"An error occured while delete list course subtitles.\n Trace: {ex.Message}");
                throw new Exception($"An error occured while delete list course subtitles.");
            }
        }
    }
}