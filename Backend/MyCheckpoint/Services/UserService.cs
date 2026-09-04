using AutoMapper;
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
        private readonly UserManager<User> _userManager;
        private readonly IFollowRepository _followRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IRepositoryBaseWithId<Category> _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(
            UserManager<User> userManager,
            IFollowRepository followRepository,
            INotificationRepository notificationRepository,
            IRepositoryBaseWithId<Category> categoryRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userManager = userManager;
            _followRepository = followRepository;
            _notificationRepository = notificationRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserSummaryDto> GetByIdAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString())
                ?? throw new NotFoundException("El usuario no existe.");

            return _mapper.Map<UserSummaryDto>(user);
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

            // Followeds = filas de Follow donde este usuario es el "Followed",
            // es decir, la gente que LE sigue a él (contraintuitivo por el
            // nombre, pero así está modelada la relación en ApplicationDbContext).
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

            // Un único listado (a diferencia de top-by-favorites, que agrupa
            // por categoría): ordenado por cuántos géneros favoritos
            // compartís, y solo como desempate por nº de seguidores.
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

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("El usuario no existe.");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        public async Task DeleteMyAccountAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("El usuario no existe.");

            // Follow tiene OnDelete(Restrict) en sus dos relaciones con User
            // (evita el problema de doble cascada sobre la misma tabla), así
            // que hay que borrar a mano las filas de follows antes de poder
            // borrar al usuario.
            var followers = await _followRepository.GetFollowersAsync(userId);
            var following = await _followRepository.GetFollowingAsync(userId);
            foreach (var follow in followers.Concat(following))
                _followRepository.Delete(follow);

            // Notification tiene el mismo problema de doble FK a User que
            // Follow: hay que borrar a mano tanto las que recibió como las
            // que generó él mismo.
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
