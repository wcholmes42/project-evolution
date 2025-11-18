# Script to analyze TMX files and output tile usage
$code = @'
using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

// Decode TMX data
string[] tmxFiles = new[]
{
    "Assets/Tilesets/extracted/Map/sample_map.tmx",
    "Assets/Tilesets/extracted/Map/sample_indoor.tmx"
};

foreach (var tmxPath in tmxFiles)
{
    if (!File.Exists(tmxPath)) continue;

    Console.WriteLine($"\n========================================");
    Console.WriteLine($"Analyzing: {Path.GetFileName(tmxPath)}");
    Console.WriteLine($"========================================\n");

    var doc = XDocument.Load(tmxPath);
    var layers = doc.Root.Elements("layer").ToList();

    foreach (var layer in layers)
    {
        string layerName = layer.Attribute("name")?.Value ?? "Unknown";
        Console.WriteLine($"\n--- Layer: {layerName} ---");

        var data = layer.Element("data");
        if (data == null) continue;

        string base64Data = data.Value.Trim().Replace("\n", "").Replace("\r", "").Replace(" ", "");
        byte[] compressedData = Convert.FromBase64String(base64Data);

        using var compressedStream = new MemoryStream(compressedData, 2, compressedData.Length - 2);
        using var decompressor = new DeflateStream(compressedStream, CompressionMode.Decompress);
        using var decompressedStream = new MemoryStream();

        decompressor.CopyTo(decompressedStream);
        byte[] decompressedData = decompressedStream.ToArray();

        int tileCount = decompressedData.Length / 4;
        var tileCounts = new Dictionary<int, int>();

        for (int i = 0; i < tileCount; i++)
        {
            int tileId = BitConverter.ToInt32(decompressedData, i * 4);
            if (tileId == 0) continue;

            if (!tileCounts.ContainsKey(tileId))
                tileCounts[tileId] = 0;

            tileCounts[tileId]++;
        }

        Console.WriteLine($"  Unique tiles: {tileCounts.Count}");
        Console.WriteLine($"\n  Top 10 tiles:");

        foreach (var tile in tileCounts.OrderByDescending(kvp => kvp.Value).Take(10))
        {
            int zeroBasedId = tile.Key - 1;
            int row = zeroBasedId / 57;
            int col = zeroBasedId % 57;

            Console.WriteLine($"    Tile {tile.Key,4} (0-based: {zeroBasedId,4} = row {row,2}, col {col,2}) - used {tile.Value,5} times");
        }
    }
}
'@

# Save and run
$code | Out-File -FilePath "temp_analyze.csx" -Encoding UTF8
dotnet script temp_analyze.csx
Remove-Item temp_analyze.csx
