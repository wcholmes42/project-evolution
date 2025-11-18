using System.Text;

namespace ProjectEvolution.Game;

/// <summary>
/// A robust UI framework for console applications that handles box-drawing,
/// Unicode characters, and emojis with proper display width calculations.
/// </summary>
public static class UIFramework
{
    // Box drawing characters
    private const char TopLeft = '╔';
    private const char TopRight = '╗';
    private const char BottomLeft = '╚';
    private const char BottomRight = '╝';
    private const char Horizontal = '═';
    private const char Vertical = '║';
    private const char LeftJoin = '╠';
    private const char RightJoin = '╣';

    // Configuration for emoji width - adjust if your terminal renders emojis differently
    public static EmojiWidthMode CurrentEmojiWidthMode = EmojiWidthMode.DoubleWidth;

    public enum EmojiWidthMode
    {
        StringLength,     // Use actual string length (emojis count as 2 chars - their surrogate pair length)
        SingleWidth,      // Emojis display as 1 character wide
        DoubleWidth,      // Emojis display as 2 characters wide (standard)
        CodePointCount    // Count Unicode code points (emojis = 1, regular = 1)
    }

    /// <summary>
    /// Calculate the actual display width of a string, accounting for emojis and wide characters.
    /// Emojis and many Unicode characters display as 2 columns wide.
    /// </summary>
    public static int GetDisplayWidth(string text)
    {
        switch (CurrentEmojiWidthMode)
        {
            case EmojiWidthMode.StringLength:
                return text.Length; // Simplest - just use string length

            case EmojiWidthMode.CodePointCount:
                return GetCodePointCount(text); // Count Unicode code points

            case EmojiWidthMode.SingleWidth:
                return GetCodePointCount(text); // Emojis = 1 char each

            case EmojiWidthMode.DoubleWidth:
            default:
                return GetDisplayWidthDoubleWide(text); // Emojis = 2 chars each
        }
    }

    private static int GetCodePointCount(string text)
    {
        int count = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                count++; // Surrogate pair = 1 code point
                i++; // Skip low surrogate
            }
            else
            {
                count++; // Regular char = 1 code point
            }
        }
        return count;
    }

    private static int GetDisplayWidthDoubleWide(string text)
    {
        int width = 0;
        for (int i = 0; i < text.Length; i++)
        {
            // Handle surrogate pairs (emojis are typically encoded as surrogate pairs in UTF-16)
            if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                // This is a surrogate pair - convert to code point
                int codePoint = char.ConvertToUtf32(text[i], text[i + 1]);
                if (IsWideCharacter(codePoint))
                    width += 2;
                else
                    width += 1;
                i++; // Skip the low surrogate
            }
            else
            {
                // Regular character
                int codePoint = text[i];
                if (IsWideCharacter(codePoint))
                    width += 2;
                else
                    width += 1;
            }
        }
        return width;
    }

    /// <summary>
    /// Check if a character displays as 2 columns wide (emojis, CJK characters, etc.)
    /// </summary>
    private static bool IsWideCharacter(int codePoint)
    {
        // Emoji ranges and other wide characters
        return (codePoint >= 0x1F300 && codePoint <= 0x1F9FF) || // Emojis and symbols
               (codePoint >= 0x2600 && codePoint <= 0x26FF) ||   // Miscellaneous symbols
               (codePoint >= 0x2700 && codePoint <= 0x27BF) ||   // Dingbats
               (codePoint >= 0xFE00 && codePoint <= 0xFE0F) ||   // Variation selectors
               (codePoint >= 0x1F000 && codePoint <= 0x1FFFF) || // Supplementary symbols and pictographs
               (codePoint >= 0x3000 && codePoint <= 0x303F) ||   // CJK symbols
               (codePoint >= 0x4E00 && codePoint <= 0x9FFF) ||   // CJK unified ideographs
               (codePoint >= 0xAC00 && codePoint <= 0xD7AF) ||   // Hangul syllables
               (codePoint >= 0x2190 && codePoint <= 0x21FF);     // Arrows (→)
    }

    /// <summary>
    /// Pad a string to a specific display width (accounting for emojis).
    /// </summary>
    public static string PadToWidth(string text, int targetWidth, PadDirection direction = PadDirection.Left)
    {
        int currentWidth = GetDisplayWidth(text);
        int spacesNeeded = targetWidth - currentWidth;

        if (spacesNeeded <= 0)
            return text;

        string padding = new string(' ', spacesNeeded);

        return direction switch
        {
            PadDirection.Right => padding + text,  // Right-align: padding before text
            PadDirection.Center => new string(' ', spacesNeeded / 2) + text + new string(' ', spacesNeeded - spacesNeeded / 2),
            _ => text + padding  // Left-align (default): padding after text
        };
    }

    /// <summary>
    /// Create a box with title and optional subtitle.
    /// </summary>
    public static BoxBuilder CreateBox(int width)
    {
        return new BoxBuilder(width);
    }

    public enum PadDirection
    {
        Left,
        Right,
        Center
    }

    /// <summary>
    /// Builder class for creating boxes with content.
    /// </summary>
    public class BoxBuilder
    {
        private readonly int innerWidth;
        private readonly List<string> lines = new();
        private string? title;
        private string? subtitle;
        private bool hasDivider = false;

        public BoxBuilder(int width)
        {
            this.innerWidth = width;
        }

        public BoxBuilder WithTitle(string title, PadDirection alignment = PadDirection.Center)
        {
            this.title = PadToWidth(title, innerWidth, alignment);
            return this;
        }

        public BoxBuilder WithSubtitle(string subtitle, PadDirection alignment = PadDirection.Center)
        {
            this.subtitle = PadToWidth(subtitle, innerWidth, alignment);
            return this;
        }

        public BoxBuilder WithDivider()
        {
            this.hasDivider = true;
            return this;
        }

        public BoxBuilder AddLine(string content, PadDirection alignment = PadDirection.Left, int indent = 2)
        {
            string indented = indent > 0 ? new string(' ', indent) + content : content;
            string padded = PadToWidth(indented, innerWidth, alignment);
            lines.Add(padded);
            return this;
        }

        public BoxBuilder AddEmptyLine()
        {
            lines.Add(new string(' ', innerWidth));
            return this;
        }

        public BoxBuilder AddLines(IEnumerable<string> contents, PadDirection alignment = PadDirection.Left, int indent = 2)
        {
            foreach (var content in contents)
            {
                AddLine(content, alignment, indent);
            }
            return this;
        }

        public string Build()
        {
            var sb = new StringBuilder();

            // Top border
            sb.AppendLine($"{TopLeft}{new string(Horizontal, innerWidth)}{TopRight}");

            // Title
            if (title != null)
            {
                sb.AppendLine($"{Vertical}{title}{Vertical}");
            }

            // Subtitle
            if (subtitle != null)
            {
                sb.AppendLine($"{Vertical}{subtitle}{Vertical}");
            }

            // Divider
            if (hasDivider)
            {
                sb.AppendLine($"{LeftJoin}{new string(Horizontal, innerWidth)}{RightJoin}");
            }

            // Content lines
            foreach (var line in lines)
            {
                sb.AppendLine($"{Vertical}{line}{Vertical}");
            }

            // Bottom border
            sb.Append($"{BottomLeft}{new string(Horizontal, innerWidth)}{BottomRight}");

            return sb.ToString();
        }

        public void Render()
        {
            Console.Write(Build());
        }
    }

    /// <summary>
    /// Create a simple menu with options.
    /// </summary>
    public static string CreateMenu(string title, string subtitle, Dictionary<string, string> options, int width = 64)
    {
        var box = CreateBox(width)
            .WithTitle(title)
            .WithSubtitle($"\"{subtitle}\"")
            .WithDivider()
            .AddEmptyLine();

        foreach (var option in options)
        {
            box.AddLine($"[{option.Key}] {option.Value}");
        }

        box.AddEmptyLine();

        return box.Build();
    }

    /// <summary>
    /// Create a status panel with label-value pairs.
    /// </summary>
    public static string CreateStatusPanel(string title, Dictionary<string, string> stats, int width = 64)
    {
        var box = CreateBox(width)
            .WithTitle(title)
            .WithDivider();

        foreach (var stat in stats)
        {
            box.AddLine($"{stat.Key}: {stat.Value}");
        }

        return box.Build();
    }

    /// <summary>
    /// Create a simple message box.
    /// </summary>
    public static string CreateMessageBox(string message, int width = 64, PadDirection alignment = PadDirection.Center)
    {
        return CreateBox(width)
            .AddEmptyLine()
            .AddLine(message, alignment, indent: 0)
            .AddEmptyLine()
            .Build();
    }

    /// <summary>
    /// Draw a horizontal line separator.
    /// </summary>
    public static string CreateSeparator(int width = 64)
    {
        return new string(Horizontal, width);
    }

    /// <summary>
    /// Create a table with headers and rows.
    /// </summary>
    public static string CreateTable(string[] headers, List<string[]> rows, int width = 64)
    {
        var box = CreateBox(width);

        // Calculate column widths
        int colCount = headers.Length;
        int colWidth = (width - (colCount + 1) * 2) / colCount; // Space for separators

        // Add header
        var headerLine = string.Join(" | ", headers.Select(h => PadToWidth(h, colWidth, PadDirection.Center)));
        box.AddLine(headerLine, PadDirection.Left, indent: 1);
        box.AddLine(new string('─', width - 4), PadDirection.Left, indent: 1);

        // Add rows
        foreach (var row in rows)
        {
            var rowLine = string.Join(" | ", row.Select(r => PadToWidth(r, colWidth, PadDirection.Left)));
            box.AddLine(rowLine, PadDirection.Left, indent: 1);
        }

        return box.Build();
    }
}
