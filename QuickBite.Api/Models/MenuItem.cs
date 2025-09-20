using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuickBite.Api.Models
{
    /// <summary>
    /// Represents a menu item in the QuickBite food menu system
    /// Configured for SQLite database with Entity Framework Core
    /// </summary>
    [Table("MenuItems")]
    [Index(nameof(Category), Name = "IX_MenuItems_Category")]
    [Index(nameof(Price), Name = "IX_MenuItems_Price")]
    [Index(nameof(Name), Name = "IX_MenuItems_Name")]
    public class MenuItem
    {
        /// <summary>
        /// Unique identifier for the menu item
        /// </summary>
        [Key]
        [Column("Id", TypeName = "TEXT")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Name of the menu item
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        [Column("Name", TypeName = "TEXT")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the menu item
        /// </summary>
        [StringLength(500)]
        [Column("Description", TypeName = "TEXT")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Price of the menu item in decimal format
        /// </summary>
        [Required]
        [Column("Price", TypeName = "DECIMAL(18,2)")]
        [Precision(18, 2)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        /// <summary>
        /// Category of the menu item (e.g., appetizers, mains, desserts, beverages)
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column("Category", TypeName = "TEXT")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Dietary tags for the menu item (e.g., vegetarian, vegan, gluten-free)
        /// Stored as JSON text in SQLite database
        /// </summary>
        [ValidDietaryTags]
        [Column("DietaryTags", TypeName = "TEXT")]
        public List<string> DietaryTags { get; set; } = new List<string>();

        /// <summary>
        /// List of ingredients in the menu item
        /// Stored as JSON text in SQLite database
        /// </summary>
        [Column("Ingredients", TypeName = "TEXT")]
        public List<string> Ingredients { get; set; } = new List<string>();

        /// <summary>
        /// Timestamp when the menu item was created
        /// </summary>
        [Column("CreatedAt", TypeName = "DATETIME")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when the menu item was last updated
        /// </summary>
        [Column("UpdatedAt", TypeName = "DATETIME")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}