using backend.DTOs.admin;
using backend.DTOs.Menu;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Constants; // <-- make sure this namespace matches your AppConstants folder

namespace backend.Controllers
{
    [ApiController]
    [Route("api/menu")]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;
        private readonly ILogger<MenuController> _logger;

        public MenuController(IMenuService menuService, ILogger<MenuController> logger)
        {
            _menuService = menuService;
            _logger = logger;
        }

        // ============================
        //       ADMIN ENDPOINTS
        // ============================

        [HttpGet("admin/all")]
        [Authorize(Roles = AppConstants.Roles.Admin)]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetAllAdminItems()
        {
            _logger.LogInformation(AppConstants.Logs.MenuRequest.AdminGetAll);

            var items = await _menuService.GetAllAdminItemsAsync();
            return Ok(items);
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.Roles.Admin)]
        public async Task<ActionResult<MenuItemDto>> CreateMenuItem([FromForm] MenuItemCreateDto dto)
        {
            _logger.LogInformation(AppConstants.Logs.MenuRequest.Create, dto.Name);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(AppConstants.Logs.MenuRequest.CreateInvalidModel);
                return BadRequest(ModelState);
            }

            var createdItem = await _menuService.CreateMenuItemAsync(dto);

            return CreatedAtAction(nameof(GetMenuItemById), new { id = createdItem.ItemId }, createdItem);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = AppConstants.Roles.Admin)]
        public async Task<IActionResult> UpdateMenuItem(int id, [FromBody] MenuItemUpdateDto dto)
        {
            _logger.LogInformation(AppConstants.Logs.MenuRequest.Update, id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(AppConstants.Logs.MenuRequest.UpdateInvalidModel, id);
                return BadRequest(ModelState);
            }

            await _menuService.UpdateMenuItemAsync(id, dto);

            _logger.LogInformation(AppConstants.Logs.UpdateSuccess, id);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppConstants.Roles.Admin)]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            _logger.LogInformation(AppConstants.Logs.MenuRequest.Delete, id);

            await _menuService.DeleteMenuItemAsync(id);

            _logger.LogInformation(AppConstants.Logs.DeleteSuccess, id);

            return NoContent();
        }

        // ============================
        //       PUBLIC ENDPOINTS
        // ============================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetAllItems()
        {
            _logger.LogInformation(AppConstants.Logs.MenuRequest.GetAll);

            var items = await _menuService.GetAllAvailableItemsAsync();
            return Ok(items);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<MenuCategoryDto>>> GetAllCategories()
        {
            _logger.LogInformation(AppConstants.Logs.MenuRequest.GetCategories);

            var categories = await _menuService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetMenuItemsByCategory(int categoryId)
        {
            _logger.LogInformation(AppConstants.Logs.MenuRequest.GetByCategory, categoryId);

            var items = await _menuService.GetAvailableItemsByCategoryIdAsync(categoryId);
            return Ok(items);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> SearchMenuItems([FromQuery] string query)
        {
            _logger.LogInformation(AppConstants.Logs.MenuRequest.Search, query);

            var items = await _menuService.SearchAvailableItemsAsync(query);
            return Ok(items);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<MenuItemDto>> GetMenuItemById(int id)
        {
            _logger.LogInformation(AppConstants.Logs.MenuRequest.GetById, id);

            var item = await _menuService.GetMenuItemByIdAsync(id);

            return Ok(item);
        }
    }
}
