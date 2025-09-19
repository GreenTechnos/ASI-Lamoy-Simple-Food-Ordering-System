using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
        [ApiController]
        [Route("api/[controller]")]
        public class MenuController : ControllerBase
        {
            private readonly ApplicationDBContext _context;

            public MenuController(ApplicationDBContext context)
            {
                _context = context;
            }

            // GET: api/menu/category/{categoryId}
            [HttpGet("category/{categoryId}")]
            public async Task<ActionResult<IEnumerable<MenuItem>>> GetMenuItemsByCategory(int categoryId)
            {
                var items = await _context.MenuItems
                    .Where(mi => mi.CategoryId == categoryId && mi.IsAvailable)
                    .ToListAsync();
                return Ok(items);
            }

            // GET: api/menu/search?query=xxx
            [HttpGet("search")]
            public async Task<ActionResult<IEnumerable<MenuItem>>> SearchMenuItems([FromQuery] string query)
            {
                if (string.IsNullOrWhiteSpace(query))
                    return BadRequest("Query parameter is required.");

                var items = await _context.MenuItems
                    .Where(mi => mi.IsAvailable &&
                        (EF.Functions.Like(mi.Name, $"%{query}%") ||
                         EF.Functions.Like(mi.Description, $"%{query}%")))
                    .ToListAsync();
                return Ok(items);
            }
        }
}