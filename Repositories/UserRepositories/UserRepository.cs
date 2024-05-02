using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.UserDtos.Response;
using OnlineCoursePlatform.Models.PagingAndFilter;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.User;

namespace OnlineCoursePlatform.Repositories.UserRepositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<PagedList<UserInfoResponseDto>> GetAllUsersAsync(
            UserFilterParams pagingAndFilterParams)
        {
            var collection = InitCollection();
            collection = FilterByName(
                collection: collection,
                query: pagingAndFilterParams.Query);
            collection = FilterByProperties(
                collection: collection,
                filter: pagingAndFilterParams.UserFilterProperties);
            var pagedList = await ProcessPagingAsync(
                collection: collection,
                pageNumber: pagingAndFilterParams.PageNumber,
                pageSize: pagingAndFilterParams.PageSize);
            var result = SelectUserInfo(pagedList);
            return CreatePagedList(result, pagedList);
        }

        private IQueryable<AppUser> InitCollection()
        => _userManager.Users.AsQueryable();


        private IQueryable<AppUser> FilterByName(IQueryable<AppUser> collection, string? query)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                var words = query.Trim().ToLowerInvariant().Split(' ');

                collection = collection.Select(us => new
                {
                    User = us,
                    MatchCount = words.Count(word => EF.Functions.Like(us.Name.ToLower(), "%" + word + "%"))
                })
                    .Where(x => x.MatchCount > 0)
                    .OrderByDescending(x => x.MatchCount)
                    .Select(x => x.User);
            }
            return collection;
        }


        private IQueryable<AppUser> FilterByProperties(IQueryable<AppUser> collection, UserFilterProperties? filter)
        {
            if (filter is not null)
            {
                if (filter.RoleIds is not null)
                {
                    collection = collection.Where(user =>
                        filter.RoleIds.OrderBy(key => key)
                        .SequenceEqual(_userManager.GetRolesAsync(user).Result.OrderBy(key => key)));
                }
            }

            return collection;
        }

        private async Task<PagedList<AppUser>> ProcessPagingAsync(
            IQueryable<AppUser> collection, int pageNumber, int pageSize)
        {
            return await PagedList<AppUser>.ToPagedListAsync(
                source: collection,
                pageNumber: pageNumber,
                pageSize: pageSize);
        }

        private IQueryable<UserInfoResponseDto> SelectUserInfo(PagedList<AppUser> pagedList)
        {
            return pagedList.Select(c => new UserInfoResponseDto()
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.PhoneNumber,
                Picture = c.Picture,
                Roles = _userManager.GetRolesAsync(c).Result
            }).AsQueryable();
        }

        private PagedList<UserInfoResponseDto> CreatePagedList(
            IQueryable<UserInfoResponseDto> items, PagedList<AppUser> pagedList)
        {
            return new PagedList<UserInfoResponseDto>(
                items: items.ToList(),
                count: pagedList.TotalCount,
                pageNumber: pagedList.CurrentPage,
                pageSize: pagedList.PageSize);
        }




        public async Task<AppUser?> FindUserByIdAsync(string userId)
            => await _userManager.FindByIdAsync(userId: userId);

        public async Task<IdentityResult> AddRolesForUserAsync(AppUser user, List<string> roleNames)
        {
            // Check if all roles exist
            var checkResult = await CheckRolesExistsAsync(roleNames);
            // If all roles exist, add them to the user, otherwise return the check result
            return checkResult.Succeeded ? await _userManager.AddToRolesAsync(user, roleNames) : checkResult;
        }

        public async Task<IdentityResult> RemoveRolesForUserAsync(AppUser user, List<string> roleNames)
        {
            // Check if all roles exist
            var checkResult = await CheckRolesExistsAsync(roleNames);
            // If all roles exist, remove them from the user, otherwise return the check result
            return checkResult.Succeeded ? await _userManager.RemoveFromRolesAsync(user, roleNames) : checkResult;
        }

        private async Task<IdentityResult> CheckRolesExistsAsync(List<string> roleNames)
        {
            // Iterate over each role in roleNames
            foreach (var roleName in roleNames)
            {
                // Check if the role exists
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    // If a role does not exist, return IdentityResult with an error
                    return IdentityResult.Failed(
                        new IdentityError {
                            Code = StatusCodes.Status404NotFound.ToString(),
                            Description = $"Role '{roleName}' does not exist." });
                }
            }
            // If all roles exist, return a successful IdentityResult
            return IdentityResult.Success;
        }

        public async Task<AppUser?> GetUserAsync(string userId)
        => await _userManager.FindByIdAsync(userId: userId);
    }
}