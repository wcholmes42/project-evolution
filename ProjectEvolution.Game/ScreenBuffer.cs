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
        if (y < 0 || y >= _height) return;

        var target = _useBackBuffer ? _backBuffer : _buffer;
        var targetColor = _useBackBuffer ? _backColorBuffer : _colorBuffer;

        int endX = maxWidth > 0 ? Math.Min(x + maxWidth, _width) : _width;

        for (int i = 0; i < text.Length && x + i < endX; i++)
        {
            target[x + i, y] = text[i];
            targetColor[x + i, y] = color;
        }
    }

    public void WriteBox(int x, int y, int width, int height, string title = "", ConsoleColor color = ConsoleColor.White)
    {
        if (width < 3 || height < 3) return;

        // Top border
        WriteAt(x, y, "╔", color);
        WriteAt(x + 1, y, new string('═', width - 2), color);
        WriteAt(x + width - 1, y, "╗", color);

        // Title if provided
        if (!string.IsNullOrEmpty(title) && title.Length < width - 4)
        {
            int titleX = x + (width - title.Length - 2) / 2;
            WriteAt(titleX, y, $" {title} ", color);
        }

        // Sides
        for (int row = 1; row < height - 1; row++)
        {
            WriteAt(x, y + row, "║", color);
            WriteAt(x + width - 1, y + row, "║", color);
        }

        // Bottom border
        WriteAt(x, y + height - 1, "╚", color);
        WriteAt(x + 1, y + height - 1, new string('═', width - 2), color);
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
        var source = _useBackBuffer ? _buffer : _backBuffer;
        var sourceColor = _useBackBuffer ? _colorBuffer : _backColorBuffer;

        try
        {
            Console.CursorVisible = false;

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    // Only redraw if character or color changed
                    bool changed = source[x, y] != _backBuffer[x, y] ||
                                  sourceColor[x, y] != _backColorBuffer[x, y];

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
