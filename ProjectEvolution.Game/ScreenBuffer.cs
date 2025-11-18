using System.Text;

namespace ProjectEvolution.Game;

/// <summary>
/// Rock-solid double-buffered screen rendering system
/// Ultima 4-era AAA quality: Perfect alignment, zero flicker, full-screen layouts
/// </summary>
public class ScreenBuffer
{
    private readonly int _width;
    private readonly int _height;
    private readonly char[,] _buffer;
    private readonly ConsoleColor[,] _colorBuffer;
    private readonly char[,] _backBuffer;
    private readonly ConsoleColor[,] _backColorBuffer;
    private bool _useBackBuffer = false;

    public int Width => _width;
    public int Height => _height;

    public ScreenBuffer(int width, int height)
    {
        _width = width;
        _height = height;
        _buffer = new char[width, height];
        _colorBuffer = new ConsoleColor[width, height];
        _backBuffer = new char[width, height];
        _backColorBuffer = new ConsoleColor[width, height];

        Clear();
    }

    public void Clear(char fillChar = ' ', ConsoleColor color = ConsoleColor.Black)
    {
        var target = _useBackBuffer ? _backBuffer : _buffer;
        var targetColor = _useBackBuffer ? _backColorBuffer : _colorBuffer;

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                target[x, y] = fillChar;
                targetColor[x, y] = color;
            }
        }
    }

    public void WriteAt(int x, int y, string text, ConsoleColor color = ConsoleColor.White, int maxWidth = -1)
    {
        if (y < 0 || y >= _height || x < 0 || x >= _width) return;
        if (string.IsNullOrEmpty(text)) return;

        var target = _useBackBuffer ? _backBuffer : _buffer;
        var targetColor = _useBackBuffer ? _backColorBuffer : _colorBuffer;

        // Strict bounds: never write past screen width or specified maxWidth
        int endX = maxWidth > 0 ? Math.Min(x + maxWidth, _width) : _width;
        int availableWidth = endX - x;

        // Truncate text if it would exceed bounds
        int charsToWrite = Math.Min(text.Length, availableWidth);

        for (int i = 0; i < charsToWrite; i++)
        {
            if (x + i >= _width) break; // Extra safety check
            target[x + i, y] = text[i];
            targetColor[x + i, y] = color;
        }
    }

    public void WriteBox(int x, int y, int width, int height, string title = "", ConsoleColor color = ConsoleColor.White)
    {
        if (width < 3 || height < 3) return;
        if (x < 0 || y < 0 || x + width > _width || y + height > _height) return;

        // Top border
        WriteAt(x, y, "╔", color);
        WriteAt(x + 1, y, new string('═', Math.Max(0, width - 2)), color);
        WriteAt(x + width - 1, y, "╗", color);

        // Title if provided - truncate if too long
        if (!string.IsNullOrEmpty(title))
        {
            int maxTitleLen = width - 4;
            string truncTitle = title.Length > maxTitleLen ? title.Substring(0, maxTitleLen) : title;
            int titleX = x + (width - truncTitle.Length - 2) / 2;
            WriteAt(titleX, y, $" {truncTitle} ", color);
        }

        // Sides
        for (int row = 1; row < height - 1; row++)
        {
            WriteAt(x, y + row, "║", color);
            WriteAt(x + width - 1, y + row, "║", color);
        }

        // Bottom border
        WriteAt(x, y + height - 1, "╚", color);
        WriteAt(x + 1, y + height - 1, new string('═', Math.Max(0, width - 2)), color);
        WriteAt(x + width - 1, y + height - 1, "╝", color);
    }

    public void DrawHorizontalLine(int x, int y, int width, ConsoleColor color = ConsoleColor.Gray)
    {
        WriteAt(x, y, new string('─', width), color);
    }

    public void DrawProgressBar(int x, int y, int width, double value, double max, ConsoleColor fillColor = ConsoleColor.Green, ConsoleColor emptyColor = ConsoleColor.DarkGray)
    {
        int filled = (int)Math.Round((value / max) * width);
        filled = Math.Clamp(filled, 0, width);

        WriteAt(x, y, new string('█', filled), fillColor);
        WriteAt(x + filled, y, new string('░', width - filled), emptyColor);
    }

    public void SwapBuffers()
    {
        _useBackBuffer = !_useBackBuffer;
    }

    public void Render()
    {
        // DEMOSCENE: Only redraw changed characters (minimize flicker!)
        // CRITICAL: Read from the buffer we just WROTE to (must match WriteAt logic!)
        var source = _useBackBuffer ? _backBuffer : _buffer;
        var sourceColor = _useBackBuffer ? _backColorBuffer : _colorBuffer;

        // Compare with the OPPOSITE buffer (what's currently on screen)
        var previous = _useBackBuffer ? _buffer : _backBuffer;
        var previousColor = _useBackBuffer ? _colorBuffer : _backColorBuffer;

        try
        {
            Console.CursorVisible = false;

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    // Only redraw if character or color changed
                    bool changed = source[x, y] != previous[x, y] ||
                                  sourceColor[x, y] != previousColor[x, y];

                    if (changed || y == 0) // Always redraw first line to ensure visibility
                    {
                        Console.SetCursorPosition(x, y);
                        Console.ForegroundColor = sourceColor[x, y];
                        Console.Write(source[x, y]);
                    }
                }
            }

            Console.ResetColor();
        }
        catch
        {
            // Ignore rendering errors (window resize during render)
        }
    }

    public void RenderFull()
    {
        // Force full redraw (for initial render or after resize)
        var source = _useBackBuffer ? _backBuffer : _buffer;
        var sourceColor = _useBackBuffer ? _backColorBuffer : _colorBuffer;

        try
        {
            Console.Clear();
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);

            var sb = new StringBuilder(_width * _height);
            ConsoleColor currentColor = ConsoleColor.White;

            for (int y = 0; y < _height; y++)
            {
                Console.SetCursorPosition(0, y);

                for (int x = 0; x < _width; x++)
                {
                    if (sourceColor[x, y] != currentColor)
                    {
                        currentColor = sourceColor[x, y];
                        Console.ForegroundColor = currentColor;
                    }

                    Console.Write(source[x, y]);
                }
            }

            Console.ResetColor();
        }
        catch
        {
            // Ignore errors
        }
    }
}
