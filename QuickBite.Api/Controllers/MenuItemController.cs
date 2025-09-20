using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickBite.Api.Data;
using QuickBite.Api.Models;

namespace QuickBite.Api.Controllers
{
    /// <summary>
    /// RESTful API controller for managing menu items in the QuickBite food ordering system.
    /// Provides CRUD operations with proper validation, error handling, and data consistency.
    /// </summary>
    /// <remarks>
    /// This controller handles all menu item operations including:
    /// - Creating new menu items with validation
    /// - Retrieving individual and collections of menu items
    /// - Updating existing menu items with conflict detection
    /// - Deleting menu items with proper cleanup
    /// 
    /// All endpoints follow RESTful conventions and return appropriate HTTP status codes.
    /// Category values are automatically normalized to lowercase for database consistency.
    /// </remarks>
    [ApiController]
    [Route("api/menuitem")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class MenuItemController : ControllerBase
    {
        #region Fields and Constructor

        /// <summary>
        /// Database context for menu item operations
        /// </summary>
        private readonly QuickBiteDbContext _context;

        /// <summary>
        /// Logger instance for tracking operations and debugging
        /// </summary>
        private readonly ILogger<MenuItemController> _logger;

        /// <summary>
        /// Initializes a new instance of the MenuItemController
        /// </summary>
        /// <param name="context">Database context for data access operations</param>
        /// <param name="logger">Logger for operation tracking and error reporting</param>
        /// <exception cref="ArgumentNullException">Thrown when context or logger is null</exception>
        public MenuItemController(QuickBiteDbContext context, ILogger<MenuItemController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// Creates a new menu item in the system
        /// </summary>
        /// <param name="menuItem">The menu item data to create</param>
        /// <returns>The created menu item with generated ID and timestamps</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/menuitem
        ///     {
        ///         "name": "Margherita Pizza",
        ///         "description": "Fresh tomatoes, mozzarella, and basil",
        ///         "price": 15.99,
        ///         "category": "mains",
        ///         "dietaryTags": ["vegetarian"],
        ///         "ingredients": ["tomatoes", "mozzarella", "basil", "pizza dough"]
        ///     }
        /// 
        /// Valid categories: appetizers, mains, desserts, beverages (case-insensitive)
        /// </remarks>
        /// <response code="201">Menu item created successfully</response>
        /// <response code="400">Invalid request data or validation errors</response>
        /// <response code="409">Menu item with the same name already exists</response>
        /// <response code="500">Internal server error occurred</response>
        [HttpPost]
        [ProducesResponseType(typeof(MenuItem), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateMenuItem([FromBody] MenuItem menuItem)
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Normalize category to lowercase
            menuItem.Category = NormalizeCategory(menuItem.Category);

            // Check for duplicate name
            var existingItem = await _context.MenuItems
                .FirstOrDefaultAsync(m => m.Name == menuItem.Name);
            
            if (existingItem != null)
            {
                return Conflict(new { message = $"Menu item with name '{menuItem.Name}' already exists." });
            }

            // Set timestamps
            menuItem.CreatedAt = DateTime.UtcNow;
            menuItem.UpdatedAt = DateTime.UtcNow;

            // Save to database
            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            // Return created result
            return CreatedAtAction(
                nameof(GetMenuItem), 
                new { id = menuItem.Id }, 
                menuItem);
        }

        #endregion

        #region Read Operations

        /// <summary>
        /// Retrieves a specific menu item by its unique identifier
        /// </summary>
        /// <param name="id">The unique GUID identifier of the menu item</param>
        /// <returns>The menu item if found, otherwise NotFound</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/menuitem/11111111-1111-1111-1111-111111111111
        /// 
        /// The ID must be a valid GUID format with or without hyphens.
        /// </remarks>
        /// <response code="200">Menu item found and returned successfully</response>
        /// <response code="404">Menu item with specified ID not found</response>
        /// <response code="400">Invalid GUID format provided</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(MenuItem), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMenuItem(Guid id)
        {
            _logger.LogInformation("Retrieving menu item with ID: {MenuItemId}", id);
            
            var menuItem = await _context.MenuItems.FindAsync(id);
            
            if (menuItem == null)
            {
                _logger.LogWarning("Menu item with ID {MenuItemId} not found", id);
                return NotFound(new { message = $"Menu item with ID '{id}' not found." });
            }

            _logger.LogInformation("Successfully retrieved menu item: {MenuItemName}", menuItem.Name);
            return Ok(menuItem);
        }

        /// <summary>
        /// Retrieves all menu items from the system
        /// </summary>
        /// <returns>A collection of all menu items</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/menuitem
        /// 
        /// Returns all menu items with their complete details including ingredients and dietary tags.
        /// Results are ordered by category and then by name for consistent presentation.
        /// </remarks>
        /// <response code="200">List of menu items returned successfully (may be empty)</response>
        /// <response code="500">Internal server error occurred</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MenuItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllMenuItems()
        {
            _logger.LogInformation("Retrieving all menu items");
            
            var menuItems = await _context.MenuItems
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();
            
            _logger.LogInformation("Retrieved {Count} menu items", menuItems.Count);
            return Ok(menuItems);
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates an existing menu item with new data
        /// </summary>
        /// <param name="id">The unique GUID identifier of the menu item to update</param>
        /// <param name="menuItem">The updated menu item data including all required fields</param>
        /// <returns>The updated menu item with new timestamp</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/menuitem/11111111-1111-1111-1111-111111111111
        ///     {
        ///         "id": "11111111-1111-1111-1111-111111111111",
        ///         "name": "Updated Pizza Name",
        ///         "description": "Updated description",
        ///         "price": 16.99,
        ///         "category": "mains",
        ///         "dietaryTags": ["vegetarian", "gluten-free"],
        ///         "ingredients": ["updated", "ingredients", "list"]
        ///     }
        /// 
        /// Important: The ID in the URL must match the ID in the request body.
        /// The category will be automatically normalized to lowercase.
        /// All fields are required even if unchanged.
        /// </remarks>
        /// <response code="200">Menu item updated successfully</response>
        /// <response code="400">Invalid request data, validation errors, or ID mismatch</response>
        /// <response code="404">Menu item with specified ID not found</response>
        /// <response code="500">Internal server error occurred</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(MenuItem), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMenuItem(Guid id, [FromBody] MenuItem menuItem)
        {
            // Check if IDs match
            if (id != menuItem.Id)
            {
                return BadRequest(new { message = "ID in route does not match ID in request body." });
            }

            // Validate model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Normalize category to lowercase
            menuItem.Category = NormalizeCategory(menuItem.Category);

            // Check if item exists
            var existingItem = await _context.MenuItems.FindAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }

            // Update properties
            existingItem.Name = menuItem.Name;
            existingItem.Description = menuItem.Description;
            existingItem.Price = menuItem.Price;
            existingItem.Category = menuItem.Category;
            existingItem.DietaryTags = menuItem.DietaryTags;
            existingItem.Ingredients = menuItem.Ingredients;
            existingItem.UpdatedAt = DateTime.UtcNow;

            // Save changes
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated menu item: {MenuItemName}", existingItem.Name);
            return Ok(existingItem);
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Permanently deletes a menu item from the system
        /// </summary>
        /// <param name="id">The unique GUID identifier of the menu item to delete</param>
        /// <returns>No content if successful, NotFound if menu item doesn't exist</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     DELETE /api/menuitem/11111111-1111-1111-1111-111111111111
        /// 
        /// Warning: This operation permanently removes the menu item and cannot be undone.
        /// Consider implementing soft deletes for production systems if needed.
        /// </remarks>
        /// <response code="204">Menu item deleted successfully</response>
        /// <response code="404">Menu item with specified ID not found</response>
        /// <response code="400">Invalid GUID format provided</response>
        /// <response code="500">Internal server error occurred</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMenuItem(Guid id)
        {
            _logger.LogInformation("Attempting to delete menu item with ID: {MenuItemId}", id);
            
            var menuItem = await _context.MenuItems.FindAsync(id);
            
            if (menuItem == null)
            {
                _logger.LogWarning("Cannot delete menu item with ID {MenuItemId} - not found", id);
                return NotFound(new { message = $"Menu item with ID '{id}' not found." });
            }

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted menu item: {MenuItemName}", menuItem.Name);
            return NoContent();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Normalizes category string to lowercase and validates it against allowed values
        /// </summary>
        /// <param name="category">The category string to normalize and validate</param>
        /// <returns>The normalized category string in lowercase</returns>
        /// <exception cref="ArgumentException">Thrown when the category is not valid</exception>
        /// <remarks>
        /// Valid categories are: appetizers, mains, desserts, beverages
        /// Input is case-insensitive and will be converted to lowercase.
        /// Whitespace is trimmed from the input.
        /// </remarks>
        private string NormalizeCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return string.Empty;
            }

            var normalizedCategory = category.Trim().ToLowerInvariant();
            
            // Valid categories (must match the database constraint)
            var validCategories = new[] { "appetizers", "mains", "desserts", "beverages" };
            
            if (!validCategories.Contains(normalizedCategory))
            {
                throw new ArgumentException($"Invalid category '{category}'. Valid categories are: {string.Join(", ", validCategories)}");
            }

            return normalizedCategory;
        }

        #endregion
    }
}