using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using MyCheckpoint.Exceptions;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Services
{
    public class UserService : IUserService
    {
        private const string AdminRole = "Admin";

        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IFollowRepository _followRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserGameRepository _userGameRepository;
        private readonly IRepositoryBaseWithId<Category> _categoryRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IFollowRepository followRepository,
            INotificationRepository notificationRepository,
            IUserGameRepository userGameRepository,
            IRepositoryBaseWithId<Category> categoryRepository,
            IFileStorageService fileStorageService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _followRepository = followRepository;
            _notificationRepository = notificationRepository;
            _userGameRepository = userGameRepository;
            _categoryRepository = categoryRepository;
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserSummaryDto> GetByIdAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString())
                ?? throw new NotFoundException("El usuario no existe.");

            return _mapper.Map<UserSummaryDto>(user);
        }

        public async Task<UserStatsDto> GetStatsAsync(int userId)
        {
            if (await _userManager.FindByIdAsync(userId.ToString()) is null)
                throw new NotFoundException("El usuario no existe.");

            return new UserStatsDto
            {
                GamesCount = await _userGameRepository.CountByUserIdAsync(userId),
                AverageRate = await _userGameRepository.GetAverageRateAsync(userId),
                TopGenre = await _userGameRepository.GetTopGenreNameAsync(userId)
            };
        }

        public async Task<IEnumerable<UserSummaryDto>> SearchAsync(string? term, int? categoryId)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(u => EF.Functions.ILike(u.UserName!, $"%{term}%"));

            if (categoryId is not null)
                query = query.Where(u => u.FavoriteGenres.Any(fg => fg.CategoryId == categoryId));

            var users = await query.ToListAsync();
            return _mapper.Map<IEnumerable<UserSummaryDto>>(users);
        }

        public async Task<IEnumerable<CategoryTopUsersDto>> GetTopByFavoriteGenresAsync(int userId, int perCategory)
        {
            var user = await GetWithFavoriteGenresAsync(userId)
                ?? throw new NotFoundException("El usuario no existe.");

            var result = new List<CategoryTopUsersDto>();
            foreach (var favorite in user.FavoriteGenres)
            {
                var topUsers = await GetTopInCategoryAsync(favorite.CategoryId, userId, 0, perCategory);
                result.Add(new CategoryTopUsersDto
                {
                    Category = _mapper.Map<CategoryDto>(favorite.Category),
                    TopUsers = topUsers.ToList()
                });
            }

            return result;
        }

        public async Task<IEnumerable<UserSummaryDto>> GetTopInCategoryAsync(int categoryId, int? excludeUserId, int skip, int take)
        {
            var query = _userManager.Users
                .Where(u => u.FavoriteGenres.Any(fg => fg.CategoryId == categoryId));

            if (excludeUserId is not null)
                query = query.Where(u => u.Id != excludeUserId);

            var users = await query
                .OrderByDescending(u => u.Followeds.Count)
                .ThenBy(u => u.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return _mapper.Map<IEnumerable<UserSummaryDto>>(users);
        }

        public async Task<IEnumerable<int>> GetFavoriteGenreCategoryIdsAsync(int userId)
        {
            var user = await GetWithFavoriteGenresAsync(userId)
                ?? throw new NotFoundException("El usuario no existe.");

            return user.FavoriteGenres.Select(fg => fg.CategoryId).ToList();
        }

        public async Task<IEnumerable<UserSummaryDto>> GetSimilarUsersAsync(int userId, int skip, int take)
        {
            var myGenreIds = await GetFavoriteGenreCategoryIdsAsync(userId);

            var users = await _userManager.Users
                .Where(u => u.Id != userId && u.FavoriteGenres.Any(fg => myGenreIds.Contains(fg.CategoryId)))
                .Select(u => new
                {
                    User = u,
                    SharedGenres = u.FavoriteGenres.Count(fg => myGenreIds.Contains(fg.CategoryId))
                })
                .OrderByDescending(x => x.SharedGenres)
                .ThenByDescending(x => x.User.Followeds.Count)
                .Skip(skip)
                .Take(take)
                .Select(x => x.User)
                .ToListAsync();

            return _mapper.Map<IEnumerable<UserSummaryDto>>(users);
        }

        public async Task<IEnumerable<RankedUserDto>> GetTopByFollowersAsync(int skip, int take)
        {
            var users = await _userManager.Users
                .Include(u => u.Followeds)
                .OrderByDescending(u => u.Followeds.Count)
                .ThenBy(u => u.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return users.Select(u => new RankedUserDto
            {
                User = _mapper.Map<UserSummaryDto>(u),
                FollowerCount = u.Followeds.Count
            });
        }

        public async Task<UserDto> GetMyProfileAsync(int userId)
        {
            var user = await GetWithFavoriteGenresAsync(userId)
                ?? throw new NotFoundException("El usuario no existe.");

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return userDto;
        }

        public async Task<UserDto> UpdateProfileAsync(int userId, UserModifyDto dto)
        {
            var user = await GetWithFavoriteGenresAsync(userId)
                ?? throw new NotFoundException("El usuario no existe.");

            _mapper.Map(dto, user);

            var categoryLookup = await GetCategoriesByIdsAsync(dto.FavoriteGenreCategoryIds);

            user.FavoriteGenres.RemoveAll(fg => !dto.FavoriteGenreCategoryIds.Contains(fg.CategoryId));

            var existingIds = user.FavoriteGenres.Select(fg => fg.CategoryId).ToHashSet();
            foreach (var categoryId in dto.FavoriteGenreCategoryIds.Where(id => !existingIds.Contains(id)))
            {
                user.FavoriteGenres.Add(new UserFavoriteGenre
                {
                    UserId = userId,
                    CategoryId = categoryId,
                    Category = categoryLookup[categoryId]
                });
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return userDto;
        }

        public async Task<UserDto> UpdateProfilePictureAsync(int userId, IFormFile file)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("El usuario no existe.");

            var previousUrl = user.ProfilePictureURL;
            user.ProfilePictureURL = await _fileStorageService.SaveProfilePictureAsync(userId, file);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));

            _fileStorageService.DeleteManagedFile(previousUrl);

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> RemoveProfilePictureAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("El usuario no existe.");

            var previousUrl = user.ProfilePictureURL;
            user.ProfilePictureURL = null;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));

            _fileStorageService.DeleteManagedFile(previousUrl);

            return _mapper.Map<UserDto>(user);
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("El usuario no existe.");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        public Task DeleteMyAccountAsync(int userId) => DeleteUserCoreAsync(userId);

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();

            var dtos = new List<UserDto>();
            foreach (var user in users)
            {
                var dto = _mapper.Map<UserDto>(user);
                dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                dtos.Add(dto);
            }
            return dtos;
        }

        public Task DeleteUserAsync(int userId) => DeleteUserCoreAsync(userId);

        public async Task<UserDto> SetAdminRoleAsync(int userId, bool isAdmin)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("El usuario no existe.");

            var isCurrentlyAdmin = await _userManager.IsInRoleAsync(user, AdminRole);

            if (isAdmin && !isCurrentlyAdmin)
            {
                if (!await _roleManager.RoleExistsAsync(AdminRole))
                    await _roleManager.CreateAsync(new Role { Name = AdminRole });

                await _userManager.AddToRoleAsync(user, AdminRole);
            }
            else if (!isAdmin && isCurrentlyAdmin)
            {
                await _userManager.RemoveFromRoleAsync(user, AdminRole);
            }

            var dto = _mapper.Map<UserDto>(user);
            dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return dto;
        }

        private async Task DeleteUserCoreAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("El usuario no existe.");

            var followers = await _followRepository.GetFollowersAsync(userId);
            var following = await _followRepository.GetFollowingAsync(userId);
            foreach (var follow in followers.Concat(following))
                _followRepository.Delete(follow);

            var notifications = await _notificationRepository.GetAllForUserAsync(userId);
            foreach (var notification in notifications)
                _notificationRepository.Delete(notification);

            await _unitOfWork.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        private Task<User?> GetWithFavoriteGenresAsync(int userId)
        {
            return _userManager.Users
                .Include(u => u.FavoriteGenres)
                    .ThenInclude(fg => fg.Category)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        private async Task<Dictionary<int, Category>> GetCategoriesByIdsAsync(IEnumerable<int> categoryIds)
        {
            var allCategories = await _categoryRepository.GetAllAsync();
            var lookup = allCategories.ToDictionary(c => c.Id);

            if (categoryIds.Any(id => !lookup.ContainsKey(id)))
                throw new NotFoundException("Una o más categorías no existen.");

            return lookup;
        }
    }
}
