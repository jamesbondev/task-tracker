namespace TaskTracker.Api.Utils;

/// <summary>
/// Provides utilities for tag normalization and common tag constants.
/// </summary>
public static class TagUtility
{
    /// <summary>
    /// Normalizes a list of tags by trimming whitespace, converting to lowercase,
    /// removing duplicates, and filtering out empty or whitespace-only tags.
    /// </summary>
    /// <param name="tags">The list of tags to normalize.</param>
    /// <returns>A normalized list of tags.</returns>
    public static List<string> NormalizeTags(List<string> tags)
    {
        return tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Common tag names used in tests and seeded data.
    /// </summary>
    public static class CommonTags
    {
        public const string Bug = "bug";
        public const string Frontend = "frontend";
        public const string Backend = "backend";
        public const string Urgent = "urgent";
        public const string Api = "api";
        public const string Setup = "setup";
        public const string Infrastructure = "infrastructure";
        public const string Testing = "testing";
        public const string Reviewed = "reviewed";
    }
}
