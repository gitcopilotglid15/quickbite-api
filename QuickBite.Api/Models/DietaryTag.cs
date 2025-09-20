using System.ComponentModel.DataAnnotations;

namespace QuickBite.Api.Models
{
    /// <summary>
    /// Enumeration of valid dietary tags
    /// </summary>
    public enum DietaryTag
    {
        /// <summary>
        /// Vegetarian diet - no meat or fish
        /// </summary>
        Vegetarian,

        /// <summary>
        /// Vegan diet - no animal products
        /// </summary>
        Vegan,

        /// <summary>
        /// Gluten-free diet
        /// </summary>
        GlutenFree,

        /// <summary>
        /// Dairy-free diet
        /// </summary>
        DairyFree,

        /// <summary>
        /// Nut-free diet
        /// </summary>
        NutFree,

        /// <summary>
        /// Halal dietary requirements
        /// </summary>
        Halal,

        /// <summary>
        /// Kosher dietary requirements
        /// </summary>
        Kosher,

        /// <summary>
        /// Low carb diet
        /// </summary>
        LowCarb,

        /// <summary>
        /// Keto diet
        /// </summary>
        Keto,

        /// <summary>
        /// Spicy food
        /// </summary>
        Spicy
    }

    /// <summary>
    /// Helper class for dietary tag operations
    /// </summary>
    public static class DietaryTagExtensions
    {
        /// <summary>
        /// Gets all available dietary tags as strings
        /// </summary>
        /// <returns>Array of dietary tag names</returns>
        public static string[] GetAllDietaryTags()
        {
            return Enum.GetNames(typeof(DietaryTag))
                       .Select(tag => ConvertToStandardFormat(tag))
                       .ToArray();
        }

        /// <summary>
        /// Validates if a dietary tag string is valid
        /// </summary>
        /// <param name="tag">Tag string to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidDietaryTag(string tag)
        {
            var normalizedTag = ConvertFromStandardFormat(tag);
            return Enum.TryParse<DietaryTag>(normalizedTag, true, out _);
        }

        /// <summary>
        /// Converts a string to DietaryTag enum
        /// </summary>
        /// <param name="tag">Tag string</param>
        /// <returns>DietaryTag enum value</returns>
        public static DietaryTag ToDietaryTag(this string tag)
        {
            var normalizedTag = ConvertFromStandardFormat(tag);
            if (Enum.TryParse<DietaryTag>(normalizedTag, true, out var result))
            {
                return result;
            }
            throw new ArgumentException($"Invalid dietary tag: {tag}");
        }

        /// <summary>
        /// Converts DietaryTag enum to standard format string (kebab-case)
        /// </summary>
        /// <param name="tag">DietaryTag enum</param>
        /// <returns>Standard format tag string</returns>
        public static string ToStandardString(this DietaryTag tag)
        {
            return ConvertToStandardFormat(tag.ToString());
        }

        /// <summary>
        /// Converts PascalCase to kebab-case (e.g., GlutenFree -> gluten-free)
        /// </summary>
        /// <param name="input">PascalCase string</param>
        /// <returns>kebab-case string</returns>
        private static string ConvertToStandardFormat(string input)
        {
            return string.Concat(input.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x : x.ToString()))
                        .ToLowerInvariant();
        }

        /// <summary>
        /// Converts kebab-case to PascalCase (e.g., gluten-free -> GlutenFree)
        /// </summary>
        /// <param name="input">kebab-case string</param>
        /// <returns>PascalCase string</returns>
        private static string ConvertFromStandardFormat(string input)
        {
            return string.Join("", input.Split('-')
                                       .Select(word => char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant()));
        }
    }

    /// <summary>
    /// Validation attribute for dietary tags
    /// </summary>
    public class ValidDietaryTagsAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is null)
                return true;

            if (value is List<string> tags)
            {
                return tags.All(tag => DietaryTagExtensions.IsValidDietaryTag(tag));
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            var validTags = string.Join(", ", DietaryTagExtensions.GetAllDietaryTags());
            return $"The {name} field contains invalid dietary tags. Valid tags are: {validTags}";
        }
    }
}