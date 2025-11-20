using backend.DTOs.admin;
using backend.DTOs.Menu;
using backend.Models;
using backend.Repositories;
using backend.Constants;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<MenuService> _logger;

        public MenuService(IMenuRepository menuRepository, IFileStorageService fileStorageService, ILogger<MenuService> logger)
        {
            _menuRepository = menuRepository;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<MenuItemDto> GetMenuItemByIdAsync(int id)
        {
            _logger.LogInformation(AppConstants.Logs.FetchById, id);

            var item = await _menuRepository.GetItemByIdAsync(id);

            if (item == null)
            {
                _logger.LogWarning(AppConstants.Logs.FetchFail, id);
                throw new KeyNotFoundException(AppConstants.Messages.MenuNotFound);
            }

            _logger.LogInformation(AppConstants.Logs.FetchSuccess, id);
            return MapToMenuItemDto(item);
        }

        public async Task<MenuItemDto> CreateMenuItemAsync(MenuItemCreateDto dto)
        {
            _logger.LogInformation(AppConstants.Logs.CreateAttempt, dto.Name);

            var category = await _menuRepository.GetCategoryByNameAsync(dto.Category);
            if (category == null)
            {
                _logger.LogWarning(AppConstants.Logs.CategoryMissing, dto.Category);
                throw new InvalidOperationException(string.Format(AppConstants.Messages.CategoryNotFound, dto.Category));
            }

            string? imageUrl = null;

            if (dto.Image != null && dto.Image.Length > 0)
            {
                _logger.LogInformation(AppConstants.Logs.UploadingImage, dto.Name);

                try
                {
                    imageUrl = await _fileStorageService.SaveFileAsync(dto.Image, AppConstants.Storage.UploadsFolder);
                    _logger.LogInformation(AppConstants.Logs.ImageSaved, imageUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, AppConstants.Logs.ImageError, dto.Name);
                    throw new InvalidOperationException(AppConstants.Messages.ImageSaveError, ex);
                }
            }

            var newItem = new MenuItem
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                ImageUrl = imageUrl,
                CategoryId = category.CategoryId,
                IsAvailable = dto.IsActive
            };

            var createdItem = await _menuRepository.CreateMenuItemAsync(newItem);

            return MapToMenuItemDto(createdItem);
        }

        public async Task UpdateMenuItemAsync(int id, MenuItemUpdateDto dto)
        {
            _logger.LogInformation(AppConstants.Logs.UpdateAttempt, id);

            var existingItem = await _menuRepository.GetItemByIdAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning(AppConstants.Logs.UpdateFail, id);
                throw new KeyNotFoundException(AppConstants.Messages.MenuNotFound);
            }

            existingItem.Name = dto.Name;
            existingItem.Description = dto.Description;
            existingItem.Price = dto.Price;
            existingItem.ImageUrl = dto.ImageUrl;
            existingItem.CategoryId = dto.CategoryId;
            existingItem.IsAvailable = dto.IsAvailable;

            await _menuRepository.UpdateMenuItemAsync();

            _logger.LogInformation(AppConstants.Logs.UpdateSuccess, id);
        }

        public async Task DeleteMenuItemAsync(int id)
        {
            _logger.LogInformation(AppConstants.Logs.DeleteAttempt, id);

            var item = await _menuRepository.GetItemByIdAsync(id);

            if (item == null)
            {
                _logger.LogWarning(AppConstants.Logs.DeleteFail, id);
                throw new KeyNotFoundException(AppConstants.Messages.MenuNotFound);
            }

            await _menuRepository.DeleteMenuItemAsync(item);

            _logger.LogInformation(AppConstants.Logs.DeleteSuccess, id);
        }

        public async Task<IEnumerable<MenuItemDto>> GetAllAvailableItemsAsync()
        {
            _logger.LogInformation(AppConstants.Logs.FetchAll);

            var items = await _menuRepository.GetAllAvailableItemsAsync();
            return items.Select(MapToMenuItemDto);
        }

        public async Task<IEnumerable<MenuItemDto>> GetAllAdminItemsAsync()
        {
            _logger.LogInformation(AppConstants.Logs.FetchAllAdmin);

            var items = await _menuRepository.GetAllAdminItemsAsync();
            return items.Select(MapToMenuItemDto);
        }

        public async Task<IEnumerable<MenuCategoryDto>> GetAllCategoriesAsync()
        {
            _logger.LogInformation(AppConstants.Logs.FetchCategories);

            var categories = await _menuRepository.GetAllCategoriesAsync();
            return categories.Select(c => new MenuCategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name
            });
        }

        public async Task<IEnumerable<MenuItemDto>> GetAvailableItemsByCategoryIdAsync(int categoryId)
        {
            var items = await _menuRepository.GetAvailableItemsByCategoryIdAsync(categoryId);
            return items.Select(MapToMenuItemDto);
        }

        public async Task<IEnumerable<MenuItemDto>> SearchAvailableItemsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning(AppConstants.Logs.SearchEmpty);
                throw new InvalidOperationException(AppConstants.Messages.QueryRequired);
            }

            _logger.LogInformation(AppConstants.Logs.SearchAttempt, query);

            var items = await _menuRepository.SearchAvailableItemsAsync(query);
            return items.Select(MapToMenuItemDto);
        }

        private MenuItemDto MapToMenuItemDto(MenuItem item)
        {
            return new MenuItemDto
            {
                ItemId = item.ItemId,
                Name = item.Name,
                Description = item.Description ?? string.Empty,
                Price = item.Price,
                ImageUrl = item.ImageUrl,
                CategoryId = item.CategoryId,
                IsAvailable = item.IsAvailable
            };
        }
    }
}
