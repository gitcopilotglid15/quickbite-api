using Microsoft.EntityFrameworkCore;
using QuickBite.Api.Data;

namespace QuickBite.Api.Extensions
{
    /// <summary>
    /// Extension methods for configuring database services
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Adds QuickBite database context to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Application configuration</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddQuickBiteDatabase(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Data Source=quickbite.db";

            services.AddDbContext<QuickBiteDbContext>(options =>
            {
                options.UseSqlite(connectionString, sqliteOptions =>
                {
                    // Configure SQLite-specific options
                    sqliteOptions.CommandTimeout(30);
                });

                // Configure Entity Framework options
                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(false);
                
#if DEBUG
                // Enable detailed logging in development
                options.EnableSensitiveDataLogging(true);
                options.EnableDetailedErrors(true);
                options.LogTo(Console.WriteLine, LogLevel.Information);
#endif
            });

            return services;
        }

        /// <summary>
        /// Ensures the database is created and migrated
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <returns>Application builder for chaining</returns>
        public static IApplicationBuilder UseQuickBiteDatabase(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<QuickBiteDbContext>();

            try
            {
                // Ensure database is created
                context.Database.EnsureCreated();

                // Apply any pending migrations
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<QuickBiteDbContext>>();
                logger.LogError(ex, "An error occurred while ensuring the database was created and migrated");
                throw;
            }

            return app;
        }

        /// <summary>
        /// Seeds the database with initial data if empty
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <returns>Application builder for chaining</returns>
        public static IApplicationBuilder SeedQuickBiteDatabase(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<QuickBiteDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<QuickBiteDbContext>>();

            try
            {
                // Only seed if the database is empty
                if (!context.MenuItems.Any())
                {
                    logger.LogInformation("Database is empty, seeding with initial data...");
                    
                    // The seed data is already defined in the DbContext OnModelCreating method
                    // This will trigger the seeding when SaveChanges is called
                    context.SaveChanges();
                    
                    logger.LogInformation("Database seeding completed successfully");
                }
                else
                {
                    logger.LogInformation("Database already contains data, skipping seeding");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }

            return app;
        }
    }
}