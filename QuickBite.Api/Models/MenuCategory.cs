namespace QuickBite.Api.Models
{
    /// <summary>
    /// Enumeration of valid menu item categories
    /// </summary>
    public enum MenuCategory
    {
        /// <summary>
        /// Appetizer or starter dishes
        /// </summary>
        Appetizers,

        /// <summary>
        /// Main course dishes
        /// </summary>
        Mains,

        /// <summary>
        /// Dessert items
        /// </summary>
        Desserts,

        /// <summary>
        /// Beverage items
        /// </summary>
        Beverages
    }

    /// <summary>
    /// Helper class for menu category operations
    /// </summary>
    public static class MenuCategoryExtensions
    {
        /// <summary>
        /// Gets all available menu categories as strings
        /// </summary>
        /// <returns>Array of category names</returns>
        public static string[] GetAllCategories()
        {
            return Enum.GetNames(typeof(MenuCategory))
                       .Select(c => c.ToLowerInvariant())
                       .ToArray();
        }

        /// <summary>
        /// Validates if a category string is valid
        /// </summary>
        /// <param name="category">Category string to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidCategory(string category)
        {
            return Enum.TryParse<MenuCategory>(category, true, out _);
        }

        /// <summary>
        /// Converts a string to MenuCategory enum
        /// </summary>
        /// <param name="category">Category string</param>
        /// <returns>MenuCategory enum value</returns>
        public static MenuCategory ToMenuCategory(this string category)
        {
            if (Enum.TryParse<MenuCategory>(category, true, out var result))
            {
                return result;
            }
            throw new ArgumentException($"Invalid category: {category}");
        }

        /// <summary>
        /// Converts MenuCategory enum to lowercase string
        /// </summary>
        /// <param name="category">MenuCategory enum</param>
        /// <returns>Lowercase category string</returns>
        public static string ToLowerString(this MenuCategory category)
        {
            return category.ToString().ToLowerInvariant();
        }
    }
}