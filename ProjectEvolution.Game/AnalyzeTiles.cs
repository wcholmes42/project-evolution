using System.IO.Compression;
using System.Xml.Linq;

namespace ProjectEvolution.Game;

/// <summary>
/// Standalone tile analysis that outputs to console for capture
/// </summary>
public class AnalyzeTiles
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Analyze both sample maps
        string[] tmxFiles =
        {
            "Assets/Tilesets/extracted/Map/sample_map.tmx",
            "Assets/Tilesets/extracted/Map/sample_indoor.tmx"
        };

        Console.WriteLine("TILE ANALYSIS RESULTS");
        Console.WriteLine("====================\n");

        foreach (var tmxPath in tmxFiles)
        {
            if (!File.Exists(tmxPath)) continue;

            Console.WriteLine($"\n=== {Path.GetFileName(tmxPath)} ===\n");

            var doc = XDocument.Load(tmxPath);
            var layers = doc.Root?.Elements("layer").ToList() ?? new List<XElement>();

            foreach (var layer in layers)
            {
                string layerName = layer.Attribute("name")?.Value ?? "Unknown";
                var data = layer.Element("data");
                if (data == null) continue;

                var tileIds = DecodeTMXData(data.Value);
                var tileCounts = new Dictionary<int, int>();

                foreach (var tileId in tileIds)
                {
                    if (tileId == 0) continue;
                    if (!tileCounts.ContainsKey(tileId))
                        tileCounts[tileId] = 0;
                    tileCounts[tileId]++;
                }

                Console.WriteLine($"Layer: {layerName}");
                Console.WriteLine($"Top 15 tiles:");

                foreach (var tile in tileCounts.OrderByDescending(kvp => kvp.Value).Take(15))
                {
                    int zeroBasedId = tile.Key - 1;
                    int row = zeroBasedId / 57;
                    int col = zeroBasedId % 57;
                    Console.WriteLine($"  Tile {tile.Key,4} (0-based: {zeroBasedId,4}, row {row,2}, col {col,2}) - {tile.Value,5}x");
                }
                Console.WriteLine();
            }
        }
    }

    private static int[] DecodeTMXData(string base64Data)
    {
        base64Data = base64Data.Trim().Replace("\n", "").Replace("\r", "").Replace(" ", "");
        byte[] compressedData = Convert.FromBase64String(base64Data);

        using var compressedStream = new MemoryStream(compressedData, 2, compressedData.Length - 2);
        using var decompressor = new DeflateStream(compressedStream, CompressionMode.Decompress);
        using var decompressedStream = new MemoryStream();

        decompressor.CopyTo(decompressedStream);
        byte[] decompressedData = decompressedStream.ToArray();

        int tileCount = decompressedData.Length / 4;
        int[] tileIds = new int[tileCount];

        for (int i = 0; i < tileCount; i++)
        {
            tileIds[i] = BitConverter.ToInt32(decompressedData, i * 4);
        }

        return tileIds;
    }
}
