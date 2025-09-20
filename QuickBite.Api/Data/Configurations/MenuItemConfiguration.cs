using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickBite.Api.Models;
using System.Text.Json;

namespace QuickBite.Api.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for MenuItem entity
    /// Provides detailed SQLite-specific configuration
    /// </summary>
    public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
    {
        public void Configure(EntityTypeBuilder<MenuItem> builder)
        {
            // Table configuration with check constraints
            builder.ToTable("MenuItems", t =>
            {
                t.HasCheckConstraint("CK_MenuItems_Price_Positive", "[Price] > 0");
                t.HasCheckConstraint("CK_MenuItems_Name_NotEmpty", "length(trim([Name])) > 0");
                t.HasCheckConstraint("CK_MenuItems_Category_Valid", 
                    "[Category] IN ('appetizers', 'mains', 'desserts', 'beverages')");
            });

            // Primary key
            builder.HasKey(e => e.Id);

            // Id configuration
            builder.Property(e => e.Id)
                .HasColumnName("Id")
                .HasColumnType("TEXT")
                .ValueGeneratedOnAdd()
                .HasComment("Unique identifier for the menu item");

            // Name configuration
            builder.Property(e => e.Name)
                .HasColumnName("Name")
                .HasColumnType("TEXT")
                .HasMaxLength(100)
                .IsRequired()
                .HasComment("Name of the menu item");

            // Description configuration
            builder.Property(e => e.Description)
                .HasColumnName("Description")
                .HasColumnType("TEXT")
                .HasMaxLength(500)
                .HasDefaultValue(string.Empty)
                .HasComment("Description of the menu item");

            // Price configuration
            builder.Property(e => e.Price)
                .HasColumnName("Price")
                .HasColumnType("DECIMAL(18,2)")
                .HasPrecision(18, 2)
                .IsRequired()
                .HasComment("Price of the menu item");

            // Category configuration
            builder.Property(e => e.Category)
                .HasColumnName("Category")
                .HasColumnType("TEXT")
                .HasMaxLength(50)
                .IsRequired()
                .HasComment("Category of the menu item");

            // DietaryTags configuration - JSON conversion
            builder.Property(e => e.DietaryTags)
                .HasColumnName("DietaryTags")
                .HasColumnType("TEXT")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, GetJsonOptions()),
                    v => JsonSerializer.Deserialize<List<string>>(v, GetJsonOptions()) ?? new List<string>())
                .HasComment("Dietary tags stored as JSON");

            // Ingredients configuration - JSON conversion
            builder.Property(e => e.Ingredients)
                .HasColumnName("Ingredients")
                .HasColumnType("TEXT")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, GetJsonOptions()),
                    v => JsonSerializer.Deserialize<List<string>>(v, GetJsonOptions()) ?? new List<string>())
                .HasComment("Ingredients stored as JSON");

            // CreatedAt configuration
            builder.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .HasColumnType("DATETIME")
                .IsRequired()
                .HasDefaultValueSql("datetime('now')")
                .ValueGeneratedOnAdd()
                .HasComment("Timestamp when the item was created");

            // UpdatedAt configuration
            builder.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .HasColumnType("DATETIME")
                .IsRequired()
                .HasDefaultValueSql("datetime('now')")
                .HasComment("Timestamp when the item was last updated");

            // Indexes for performance
            builder.HasIndex(e => e.Category)
                .HasDatabaseName("IX_MenuItems_Category")
                .HasFilter(null);

            builder.HasIndex(e => e.Price)
                .HasDatabaseName("IX_MenuItems_Price")
                .HasFilter(null);

            builder.HasIndex(e => e.Name)
                .HasDatabaseName("IX_MenuItems_Name")
                .HasFilter(null);

            // Composite index for common queries
            builder.HasIndex(e => new { e.Category, e.Price })
                .HasDatabaseName("IX_MenuItems_Category_Price")
                .HasFilter(null);
        }

        /// <summary>
        /// Gets JSON serialization options for consistent formatting
        /// </summary>
        /// <returns>JsonSerializerOptions configured for database storage</returns>
        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }
    }
}