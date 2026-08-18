namespace PCDynasty.Presentation.Core.Diagnostics;

/// <summary>
/// Turns an <see cref="ApplicationError"/> into a plain-text report
/// the player can paste into a bug ticket. Pure function so the
/// tests pin the exact format.
/// </summary>
public static class ApplicationErrorReport
{
    /// <summary>
    /// Serialises the error into a multi-line text block. The first
    /// line is the severity and title, the body follows, then the
    /// details, then the optional file path and error code at the
    /// bottom — the order the player will read when they paste.
    /// </summary>
    public static string Format(ApplicationError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        System.Text.StringBuilder builder = new();
        builder.Append('[').Append(error.Severity).Append("] ").AppendLine(error.Title);
        builder.AppendLine(error.Body);

        if (!string.IsNullOrEmpty(error.PhysicalPath))
        {
            builder.Append("Path: ").AppendLine(error.PhysicalPath);
        }
        else if (!string.IsNullOrEmpty(error.FilePath))
        {
            builder.Append("Path: ").AppendLine(error.FilePath);
        }

        if (!string.IsNullOrEmpty(error.ErrorCode))
        {
            builder.Append("Code: ").AppendLine(error.ErrorCode);
        }

        if (error.Items.Length > 0)
        {
            builder.AppendLine("Details:");
            foreach (ApplicationErrorItem item in error.Items)
            {
                builder.Append("  • ").Append(item.Label).Append(": ").AppendLine(item.Detail);
            }
        }

        return builder.ToString();
    }
}
