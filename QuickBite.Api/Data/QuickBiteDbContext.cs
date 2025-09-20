using Microsoft.EntityFrameworkCore;
using QuickBite.Api.Models;
using QuickBite.Api.Data.Configurations;
using System.Text.Json;

namespace QuickBite.Api.Data
{
    /// <summary>
    /// Entity Framework DbContext for the QuickBite food ordering system.
    /// Provides data access abstraction for SQLite database with optimized configurations.
    /// </summary>
    /// <remarks>
    /// This DbContext is specifically configured for:
    /// - SQLite database engine with performance optimizations
    /// - JSON serialization for complex types (ingredients, dietary tags)
    /// - Case-insensitive string comparisons using NOCASE collation
    /// - Automatic timestamp management for CreatedAt and UpdatedAt fields
    /// - Comprehensive indexing strategy for query performance
    /// - Database seeding with sample data for development and testing
    /// 
    /// Key Features:
    /// - Automatic entity configuration through fluent API
    /// - Thread-safe operations with proper async/await patterns
    /// - Optimistic concurrency control through timestamps
    /// - Comprehensive logging and error handling
    /// </remarks>
    public class QuickBiteDbContext : DbContext
    {
        #region Constructor and Properties

        /// <summary>
        /// Initializes a new instance of the QuickBiteDbContext with the specified options
        /// </summary>
        /// <param name="options">Database context options including connection string and provider configuration</param>
        /// <exception cref="ArgumentNullException">Thrown when options parameter is null</exception>
        public QuickBiteDbContext(DbContextOptions<QuickBiteDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the menu items entity set for CRUD operations
        /// </summary>
        /// <value>
        /// DbSet providing access to MenuItem entities with full LINQ support,
        /// change tracking, and relationship navigation capabilities
        /// </value>
        public DbSet<MenuItem> MenuItems { get; set; }

        #endregion

        #region Model Configuration

        /// <summary>
        /// Configures the database model using Entity Framework's fluent API
        /// </summary>
        /// <param name="modelBuilder">The model builder instance for configuration</param>
        /// <remarks>
        /// This method performs the following configurations:
        /// 1. Applies entity-specific configurations from separate configuration classes
        /// 2. Sets up global database settings like collation and naming conventions
        /// 3. Seeds the database with initial sample data for development and testing
        /// 
        /// Configuration is applied in a specific order to ensure dependencies are resolved correctly.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations
            modelBuilder.ApplyConfiguration(new MenuItemConfiguration());

            // Additional global configurations
            ConfigureGlobalSettings(modelBuilder);

            // Seed initial data
            SeedData(modelBuilder);
        }

        /// <summary>
        /// Configures global database settings and conventions
        /// </summary>
        /// <param name="modelBuilder">The model builder instance for global configuration</param>
        /// <remarks>
        /// Applies the following global settings:
        /// - Sets NOCASE collation for case-insensitive string operations
        /// - Configures all string properties to use case-insensitive comparisons
        /// - Establishes consistent naming conventions across all entities
        /// 
        /// These settings ensure consistent behavior across different SQLite installations
        /// and provide optimal performance for text-based queries.
        /// </remarks>
        private void ConfigureGlobalSettings(ModelBuilder modelBuilder)
        {
            // Configure SQLite-specific settings
            modelBuilder.HasAnnotation("Relational:Collation", "NOCASE");
            
            // Set default string comparison to case-insensitive
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(string))
                    {
                        property.SetCollation("NOCASE");
                    }
                }
            }
        }

        /// <summary>
        /// Configures the database context options when not configured through dependency injection
        /// </summary>
        /// <param name="optionsBuilder">The options builder for configuring database connection</param>
        /// <remarks>
        /// This method provides fallback configuration for scenarios where the DbContext
        /// is instantiated without explicit configuration (e.g., during migrations).
        /// In production, database configuration should be provided through DI container.
        /// </remarks>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Fallback configuration if not configured in DI
                optionsBuilder.UseSqlite("Data Source=quickbite.db");
            }
        }

        #endregion

        #region Save Operations

        /// <summary>
        /// Saves all changes made in this context to the database with automatic timestamp updates
        /// </summary>
        /// <returns>The number of state entries written to the database</returns>
        /// <remarks>
        /// This override automatically updates the UpdatedAt timestamp for all modified MenuItem entities
        /// before saving changes to the database. This ensures data consistency and audit trails.
        /// </remarks>
        /// <exception cref="DbUpdateException">Thrown when an error occurs during database update</exception>
        /// <exception cref="DbUpdateConcurrencyException">Thrown when a concurrency conflict occurs</exception>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database with automatic timestamp updates
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete</param>
        /// <returns>A task representing the asynchronous save operation with the number of state entries written</returns>
        /// <remarks>
        /// This override automatically updates the UpdatedAt timestamp for all modified MenuItem entities
        /// before saving changes to the database. This ensures data consistency and audit trails.
        /// 
        /// Use this method for all database save operations to maintain consistent behavior
        /// and optimal performance in async applications.
        /// </remarks>
        /// <exception cref="DbUpdateException">Thrown when an error occurs during database update</exception>
        /// <exception cref="DbUpdateConcurrencyException">Thrown when a concurrency conflict occurs</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled</exception>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Updates the UpdatedAt timestamp for all modified MenuItem entities
        /// </summary>
        /// <remarks>
        /// This method is automatically called before any save operation to ensure
        /// that the UpdatedAt timestamp reflects the most recent modification time.
        /// Only entities in the Modified state are updated to avoid unnecessary changes.
        /// 
        /// The timestamp is set to UTC to ensure consistency across different time zones.
        /// </remarks>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<MenuItem>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Seeds the database with initial sample data for development and testing
        /// </summary>
        /// <param name="modelBuilder">The model builder instance for data seeding</param>
        /// <remarks>
        /// This method populates the database with sample menu items that demonstrate
        /// the full range of features including:
        /// - Different categories (appetizers, mains, desserts, beverages)
        /// - Various dietary tags (vegetarian, vegan, gluten-free)
        /// - Complex ingredient lists
        /// - Realistic pricing and descriptions
        /// 
        /// Seeded data uses fixed GUIDs to ensure consistency across database recreations.
        /// This data is only applied during database creation and migrations.
        /// 
        /// Sample Items:
        /// - Margherita Pizza (mains, vegetarian)
        /// - Caesar Salad (appetizers, vegetarian)  
        /// - Chocolate Cake (desserts, vegetarian)
        /// - Fresh Orange Juice (beverages, vegan, gluten-free)
        /// </remarks>
        private void SeedData(ModelBuilder modelBuilder)
        {
            var menuItems = new[]
            {
                new MenuItem
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Margherita Pizza",
                    Description = "Fresh tomatoes, mozzarella, basil on thin crust",
                    Price = 12.99m,
                    Category = "mains",
                    DietaryTags = new List<string> { "vegetarian" },
                    Ingredients = new List<string> { "tomatoes", "mozzarella", "basil", "pizza dough", "olive oil" },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new MenuItem
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Caesar Salad",
                    Description = "Crisp romaine lettuce with parmesan and croutons",
                    Price = 8.99m,
                    Category = "appetizers",
                    DietaryTags = new List<string> { "vegetarian" },
                    Ingredients = new List<string> { "romaine lettuce", "parmesan cheese", "croutons", "caesar dressing" },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new MenuItem
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Chocolate Cake",
                    Description = "Rich chocolate cake with chocolate frosting",
                    Price = 6.99m,
                    Category = "desserts",
                    DietaryTags = new List<string> { "vegetarian" },
                    Ingredients = new List<string> { "chocolate", "flour", "sugar", "eggs", "butter" },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new MenuItem
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "Fresh Orange Juice",
                    Description = "Freshly squeezed orange juice",
                    Price = 4.99m,
                    Category = "beverages",
                    DietaryTags = new List<string> { "vegan", "gluten-free" },
                    Ingredients = new List<string> { "oranges" },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            modelBuilder.Entity<MenuItem>().HasData(menuItems);
        }

        #endregion
    }
}