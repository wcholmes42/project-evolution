using System.IO.Compression;
using System.Xml.Linq;

namespace ProjectEvolution.Game;

/// <summary>
/// Analyzes TMX map files to extract tile usage patterns
/// </summary>
public static class TMXAnalyzer
{
    public static void AnalyzeTMXFile(string tmxPath)
    {
        if (!File.Exists(tmxPath))
        {
            Console.WriteLine($"ERROR: TMX file not found: {tmxPath}");
            return;
        }

        Console.WriteLine($"\n═══════════════════════════════════════════════");
        Console.WriteLine($"Analyzing: {Path.GetFileName(tmxPath)}");
        Console.WriteLine($"═══════════════════════════════════════════════\n");

        var doc = XDocument.Load(tmxPath);
        var map = doc.Element("map");

        if (map == null)
        {
            Console.WriteLine("ERROR: Invalid TMX file");
            return;
        }

        // Get map dimensions
        int width = int.Parse(map.Attribute("width")?.Value ?? "0");
        int height = int.Parse(map.Attribute("height")?.Value ?? "0");
        Console.WriteLine($"Map Size: {width}x{height}");

        // Get tileset info
        var tileset = map.Element("tileset");
        if (tileset != null)
        {
            string name = tileset.Attribute("name")?.Value ?? "Unknown";
            int tilewidth = int.Parse(tileset.Attribute("tilewidth")?.Value ?? "0");
            int tileheight = int.Parse(tileset.Attribute("tileheight")?.Value ?? "0");
            int spacing = int.Parse(tileset.Attribute("spacing")?.Value ?? "0");

            Console.WriteLine($"Tileset: {name}");
            Console.WriteLine($"Tile Size: {tilewidth}x{tileheight}");
            Console.WriteLine($"Spacing: {spacing}px");

            var image = tileset.Element("image");
            if (image != null)
            {
                string source = image.Attribute("source")?.Value ?? "";
                int imgWidth = int.Parse(image.Attribute("width")?.Value ?? "0");
                int imgHeight = int.Parse(image.Attribute("height")?.Value ?? "0");
                Console.WriteLine($"Image: {Path.GetFileName(source)} ({imgWidth}x{imgHeight})");
            }
        }

        Console.WriteLine();

        // Analyze each layer
        var layers = map.Elements("layer").ToList();
        foreach (var layer in layers)
        {
            string layerName = layer.Attribute("name")?.Value ?? "Unknown";
            Console.WriteLine($"\n--- Layer: {layerName} ---");

            var data = layer.Element("data");
            if (data == null) continue;

            string encoding = data.Attribute("encoding")?.Value ?? "";
            string compression = data.Attribute("compression")?.Value ?? "";

            if (encoding == "base64" && compression == "zlib")
            {
                try
                {
                    var tileIds = DecodeBase64Zlib(data.Value);
                    AnalyzeTileUsage(tileIds, layerName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Error decoding: {ex.Message}");
                }
            }
        }
    }

    private static int[] DecodeBase64Zlib(string base64Data)
    {
        // Remove whitespace
        base64Data = base64Data.Trim().Replace("\n", "").Replace("\r", "").Replace(" ", "");

        // Decode base64
        byte[] compressedData = Convert.FromBase64String(base64Data);

        // Decompress with zlib (skip 2-byte header)
        using var compressedStream = new MemoryStream(compressedData, 2, compressedData.Length - 2);
        using var decompressor = new DeflateStream(compressedStream, CompressionMode.Decompress);
        using var decompressedStream = new MemoryStream();

        decompressor.CopyTo(decompressedStream);
        byte[] decompressedData = decompressedStream.ToArray();

        // Convert bytes to tile IDs (4 bytes per tile, little-endian)
        int tileCount = decompressedData.Length / 4;
        int[] tileIds = new int[tileCount];

        for (int i = 0; i < tileCount; i++)
        {
            tileIds[i] = BitConverter.ToInt32(decompressedData, i * 4);
        }

        return tileIds;
    }

    private static void AnalyzeTileUsage(int[] tileIds, string layerName)
    {
        // Count tile occurrences
        var tileCounts = new Dictionary<int, int>();

        foreach (var tileId in tileIds)
        {
            if (tileId == 0) continue; // 0 means empty/transparent

            if (!tileCounts.ContainsKey(tileId))
                tileCounts[tileId] = 0;

            tileCounts[tileId]++;
        }

        // Show most used tiles
        var topTiles = tileCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(10)
            .ToList();

        Console.WriteLine($"  Total tiles: {tileIds.Length}");
        Console.WriteLine($"  Non-empty tiles: {tileIds.Count(t => t != 0)}");
        Console.WriteLine($"  Unique tiles used: {tileCounts.Count}");
        Console.WriteLine($"\n  Top 10 tiles used:");

        foreach (var tile in topTiles)
        {
            // Tile IDs in TMX are 1-based (firstgid=1), so subtract 1 for 0-based indexing
            int zeroBasedId = tile.Key - 1;
            int row = zeroBasedId / 57;
            int col = zeroBasedId % 57;

            Console.WriteLine($"    Tile {tile.Key,4} (row {row,2}, col {col,2}) - used {tile.Value,5} times");
        }
    }

    public static void AnalyzeAllSampleMaps()
    {
        // Analyze and write to both console and file
        using var fileWriter = new StreamWriter("tile-analysis.txt");
        var originalOut = Console.Out;

        try
        {
            // Write to both console and file
            var multiWriter = new MultiTextWriter(Console.Out, fileWriter);
            Console.SetOut(multiWriter);

            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("         TMX TILE ANALYSIS - Kenney Roguelike Pack");
            Console.WriteLine("═══════════════════════════════════════════════════════════");

            string[] tmxFiles =
            {
                "Assets/Tilesets/extracted/Map/sample_map.tmx",
                "Assets/Tilesets/extracted/Map/sample_indoor.tmx"
            };

            foreach (var tmxFile in tmxFiles)
            {
                AnalyzeTMXFile(tmxFile);
            }

            Console.WriteLine("\n═══════════════════════════════════════════════════════════");
            Console.WriteLine("Analysis saved to: tile-analysis.txt");
            Console.WriteLine("Press any key to exit...");
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.WriteLine("\nAnalysis saved to: tile-analysis.txt");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    // Helper class to write to multiple TextWriters
    private class MultiTextWriter : TextWriter
    {
        private readonly TextWriter[] writers;

        public MultiTextWriter(params TextWriter[] writers)
        {
            this.writers = writers;
        }

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        public override void Write(char value)
        {
            foreach (var writer in writers)
                writer.Write(value);
        }

        public override void WriteLine(string? value)
        {
            foreach (var writer in writers)
                writer.WriteLine(value);
        }
    }
}
