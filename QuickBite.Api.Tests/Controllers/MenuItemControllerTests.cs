using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using QuickBite.Api.Controllers;
using QuickBite.Api.Models;
using QuickBite.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace QuickBite.Api.Tests.Controllers
{
    /// <summary>
    /// Unit tests for MenuItemController following TDD approach
    /// </summary>
    public class MenuItemControllerTests : IDisposable
    {
        private readonly QuickBiteDbContext _context;
        private readonly Mock<ILogger<MenuItemController>> _mockLogger;
        private readonly MenuItemController _controller;

        public MenuItemControllerTests()
        {
            // Arrange - Set up test dependencies
            var options = new DbContextOptionsBuilder<QuickBiteDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new QuickBiteDbContext(options);
            _mockLogger = new Mock<ILogger<MenuItemController>>();
            _controller = new MenuItemController(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task TestCreateMenuItem_ShouldReturnSavedMenuItem()
        {
            // Arrange - Create a new menu item request
            var newMenuItem = new MenuItem
            {
                Name = "Test Pizza",
                Description = "A delicious test pizza with cheese and tomatoes",
                Price = 15.99m,
                Category = "mains",
                DietaryTags = new List<string> { "vegetarian" },
                Ingredients = new List<string> { "cheese", "tomatoes", "pizza dough" }
            };

            // Act - Call the CreateMenuItem endpoint
            var result = await _controller.CreateMenuItem(newMenuItem);

            // Assert - Verify the response
            result.Should().NotBeNull();
            
            // Should return CreatedAtActionResult (201 Created)
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.StatusCode.Should().Be(201);
            
            // Should return the created menu item
            var returnedMenuItem = createdResult.Value.Should().BeOfType<MenuItem>().Subject;
            returnedMenuItem.Should().NotBeNull();
            
            // Verify all properties are correctly set
            returnedMenuItem.Id.Should().NotBeEmpty();
            returnedMenuItem.Name.Should().Be(newMenuItem.Name);
            returnedMenuItem.Description.Should().Be(newMenuItem.Description);
            returnedMenuItem.Price.Should().Be(newMenuItem.Price);
            returnedMenuItem.Category.Should().Be(newMenuItem.Category);
            returnedMenuItem.DietaryTags.Should().BeEquivalentTo(newMenuItem.DietaryTags);
            returnedMenuItem.Ingredients.Should().BeEquivalentTo(newMenuItem.Ingredients);
            returnedMenuItem.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
            returnedMenuItem.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));

            // Verify item was actually saved to database
            var savedItem = await _context.MenuItems.FindAsync(returnedMenuItem.Id);
            savedItem.Should().NotBeNull();
            savedItem!.Name.Should().Be(newMenuItem.Name);
            savedItem.Price.Should().Be(newMenuItem.Price);

            // Verify the CreatedAtAction points to the correct action
            createdResult.ActionName.Should().Be(nameof(MenuItemController.GetMenuItem));
            createdResult.RouteValues.Should().ContainKey("id");
            createdResult.RouteValues!["id"].Should().Be(returnedMenuItem.Id);
        }

        [Fact]
        public async Task TestCreateMenuItem_WithInvalidModel_ShouldReturnBadRequest()
        {
            // Arrange - Create an invalid menu item (missing required fields)
            var invalidMenuItem = new MenuItem
            {
                // Missing Name (required)
                Description = "A pizza without a name",
                Price = -5.00m, // Invalid negative price
                Category = "invalid-category", // Invalid category
                DietaryTags = new List<string> { "invalid-tag" }, // Invalid dietary tag
                Ingredients = new List<string>()
            };

            // Simulate model validation failure
            _controller.ModelState.AddModelError("Name", "Name is required");
            _controller.ModelState.AddModelError("Price", "Price must be greater than 0");
            _controller.ModelState.AddModelError("Category", "Invalid category");

            // Act - Call the CreateMenuItem endpoint
            var result = await _controller.CreateMenuItem(invalidMenuItem);

            // Assert - Should return BadRequest
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.StatusCode.Should().Be(400);

            // Verify no item was saved to database
            var itemCount = await _context.MenuItems.CountAsync();
            itemCount.Should().Be(0);
        }

        [Fact]
        public async Task TestCreateMenuItem_WithDuplicateName_ShouldReturnConflict()
        {
            // Arrange - Add an existing item to the database
            var existingItem = new MenuItem
            {
                Name = "Margherita Pizza",
                Description = "Classic pizza",
                Price = 12.99m,
                Category = "mains",
                DietaryTags = new List<string> { "vegetarian" },
                Ingredients = new List<string> { "cheese", "tomatoes" }
            };

            _context.MenuItems.Add(existingItem);
            await _context.SaveChangesAsync();

            // Create a new item with the same name
            var duplicateItem = new MenuItem
            {
                Name = "Margherita Pizza", // Same name as existing item
                Description = "Another pizza with the same name",
                Price = 15.99m,
                Category = "mains",
                DietaryTags = new List<string> { "vegetarian" },
                Ingredients = new List<string> { "cheese", "tomatoes", "basil" }
            };

            // Act - Call the CreateMenuItem endpoint
            var result = await _controller.CreateMenuItem(duplicateItem);

            // Assert - Should return Conflict (409)
            result.Should().BeOfType<ConflictObjectResult>();
            var conflictResult = result as ConflictObjectResult;
            conflictResult!.StatusCode.Should().Be(409);

            // Verify only one item exists in database
            var itemCount = await _context.MenuItems.CountAsync();
            itemCount.Should().Be(1);
        }

        #region GetAllMenuItems Tests

        [Fact]
        public async Task TestGetAllMenuItems_ShouldReturnAllItems()
        {
            // Arrange - Add test items to database
            var testItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Name = "Pizza Margherita",
                    Description = "Classic pizza",
                    Price = 12.99m,
                    Category = "mains",
                    DietaryTags = new List<string> { "vegetarian" },
                    Ingredients = new List<string> { "cheese", "tomatoes" }
                },
                new MenuItem
                {
                    Name = "Caesar Salad",
                    Description = "Fresh salad",
                    Price = 8.99m,
                    Category = "appetizers",
                    DietaryTags = new List<string> { "vegetarian" },
                    Ingredients = new List<string> { "lettuce", "parmesan" }
                }
            };

            _context.MenuItems.AddRange(testItems);
            await _context.SaveChangesAsync();

            // Act - Call GetAllMenuItems
            var result = await _controller.GetAllMenuItems();

            // Assert - Verify response
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedItems = okResult!.Value.Should().BeAssignableTo<IEnumerable<MenuItem>>().Subject;
            
            returnedItems.Should().HaveCount(2);
            returnedItems.Should().Contain(item => item.Name == "Pizza Margherita");
            returnedItems.Should().Contain(item => item.Name == "Caesar Salad");
        }

        [Fact]
        public async Task TestGetAllMenuItems_WithEmptyDatabase_ShouldReturnEmptyList()
        {
            // Act - Call GetAllMenuItems on empty database
            var result = await _controller.GetAllMenuItems();

            // Assert - Should return empty list
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedItems = okResult!.Value.Should().BeAssignableTo<IEnumerable<MenuItem>>().Subject;
            returnedItems.Should().BeEmpty();
        }

        #endregion

        #region GetMenuItem Tests

        [Fact]
        public async Task TestGetMenuItem_WithValidId_ShouldReturnMenuItem()
        {
            // Arrange - Add test item to database
            var testItem = new MenuItem
            {
                Name = "Test Item",
                Description = "Test description",
                Price = 10.99m,
                Category = "mains",
                DietaryTags = new List<string> { "vegan" },
                Ingredients = new List<string> { "test ingredient" }
            };

            _context.MenuItems.Add(testItem);
            await _context.SaveChangesAsync();

            // Act - Call GetMenuItem with valid ID
            var result = await _controller.GetMenuItem(testItem.Id);

            // Assert - Should return the item
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedItem = okResult!.Value.Should().BeOfType<MenuItem>().Subject;
            
            returnedItem.Should().NotBeNull();
            returnedItem.Id.Should().Be(testItem.Id);
            returnedItem.Name.Should().Be(testItem.Name);
            returnedItem.Price.Should().Be(testItem.Price);
        }

        [Fact]
        public async Task TestGetMenuItem_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange - Use a random GUID that doesn't exist
            var nonExistentId = Guid.NewGuid();

            // Act - Call GetMenuItem with invalid ID
            var result = await _controller.GetMenuItem(nonExistentId);

            // Assert - Should return NotFoundObjectResult with message
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().NotBeNull();
        }

        #endregion

        #region UpdateMenuItem Tests

        [Fact]
        public async Task TestUpdateMenuItem_WithValidData_ShouldReturnUpdatedItem()
        {
            // Arrange - Add test item to database
            var existingItem = new MenuItem
            {
                Name = "Original Pizza",
                Description = "Original description",
                Price = 12.99m,
                Category = "mains",
                DietaryTags = new List<string> { "vegetarian" },
                Ingredients = new List<string> { "cheese", "tomatoes" }
            };

            _context.MenuItems.Add(existingItem);
            await _context.SaveChangesAsync();

            // Prepare updated data
            var updatedItem = new MenuItem
            {
                Id = existingItem.Id,
                Name = "Updated Pizza",
                Description = "Updated description",
                Price = 15.99m,
                Category = "mains",
                DietaryTags = new List<string> { "vegetarian", "spicy" },
                Ingredients = new List<string> { "cheese", "tomatoes", "pepperoni" }
            };

            // Act - Call UpdateMenuItem
            var result = await _controller.UpdateMenuItem(existingItem.Id, updatedItem);

            // Assert - Should return updated item
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedItem = okResult!.Value.Should().BeOfType<MenuItem>().Subject;
            
            returnedItem.Should().NotBeNull();
            returnedItem.Id.Should().Be(existingItem.Id);
            returnedItem.Name.Should().Be("Updated Pizza");
            returnedItem.Description.Should().Be("Updated description");
            returnedItem.Price.Should().Be(15.99m);
            returnedItem.DietaryTags.Should().Contain("spicy");
            returnedItem.UpdatedAt.Should().BeAfter(returnedItem.CreatedAt);

            // Verify item was actually updated in database
            var dbItem = await _context.MenuItems.FindAsync(existingItem.Id);
            dbItem!.Name.Should().Be("Updated Pizza");
        }

        [Fact]
        public async Task TestUpdateMenuItem_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange - Use a random GUID that doesn't exist
            var nonExistentId = Guid.NewGuid();
            var updatedItem = new MenuItem
            {
                Id = nonExistentId, // Set the ID to match the route parameter
                Name = "Updated Item",
                Description = "Updated description",
                Price = 10.99m,
                Category = "mains"
            };

            // Act - Call UpdateMenuItem with invalid ID
            var result = await _controller.UpdateMenuItem(nonExistentId, updatedItem);

            // Assert - Should return NotFound
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task TestUpdateMenuItem_WithMismatchedId_ShouldReturnBadRequest()
        {
            // Arrange - Add test item to database
            var existingItem = new MenuItem
            {
                Name = "Original Item",
                Price = 10.99m,
                Category = "mains"
            };

            _context.MenuItems.Add(existingItem);
            await _context.SaveChangesAsync();

            var updatedItem = new MenuItem
            {
                Id = Guid.NewGuid(), // Different ID than the route parameter
                Name = "Updated Item",
                Price = 15.99m,
                Category = "mains"
            };

            // Act - Call UpdateMenuItem with mismatched IDs
            var result = await _controller.UpdateMenuItem(existingItem.Id, updatedItem);

            // Assert - Should return BadRequest
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region DeleteMenuItem Tests

        [Fact]
        public async Task TestDeleteMenuItem_WithValidId_ShouldReturnNoContent()
        {
            // Arrange - Add test item to database
            var testItem = new MenuItem
            {
                Name = "Item to Delete",
                Description = "This item will be deleted",
                Price = 9.99m,
                Category = "desserts",
                DietaryTags = new List<string> { "vegetarian" },
                Ingredients = new List<string> { "sugar", "flour" }
            };

            _context.MenuItems.Add(testItem);
            await _context.SaveChangesAsync();

            // Verify item exists before deletion
            var itemExists = await _context.MenuItems.AnyAsync(m => m.Id == testItem.Id);
            itemExists.Should().BeTrue();

            // Act - Call DeleteMenuItem
            var result = await _controller.DeleteMenuItem(testItem.Id);

            // Assert - Should return NoContent
            result.Should().BeOfType<NoContentResult>();
            var noContentResult = result as NoContentResult;
            noContentResult!.StatusCode.Should().Be(204);

            // Verify item was actually deleted from database
            var itemStillExists = await _context.MenuItems.AnyAsync(m => m.Id == testItem.Id);
            itemStillExists.Should().BeFalse();
        }

        [Fact]
        public async Task TestDeleteMenuItem_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange - Use a random GUID that doesn't exist
            var nonExistentId = Guid.NewGuid();

            // Act - Call DeleteMenuItem with invalid ID
            var result = await _controller.DeleteMenuItem(nonExistentId);

            // Assert - Should return NotFoundObjectResult with message
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().NotBeNull();
        }

        #endregion

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}