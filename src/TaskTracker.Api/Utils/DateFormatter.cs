namespace TaskTracker.Api.Utils;

/// <summary>
/// Provides human-friendly date formatting utilities for task timestamps.
/// </summary>
public static class DateFormatter
{
    /// <summary>
    /// Returns a relative time string (e.g., "2 hours ago", "yesterday").
    /// </summary>
    public static string ToRelativeTime(DateTime dateTime)
    {
        var now = DateTime.UtcNow;
        var diff = now - dateTime;

        if (diff.TotalDays >= 1)
            return diff.Days == 1 ? "yesterday" : $"{diff.Days} days ago";
        if (diff.TotalHours >= 1)
            return $"{(int)diff.TotalHours} hour{((int)diff.TotalHours > 1 ? "s" : "")} ago";
        if (diff.TotalMinutes >= 1)
            return $"{(int)diff.TotalMinutes} minute{((int)diff.TotalMinutes > 1 ? "s" : "")} ago";

        return "just now";
    }

    /// <summary>
    /// Formats a DateTime as a friendly display string (e.g., "March 23, 2026").
    /// </summary>
    public static string ToFriendlyDate(DateTime dateTime)
    {
        return dateTime.ToString("MMMM d, yyyy");
    }
}
