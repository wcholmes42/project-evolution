using System.IO.Compression;
using System.Xml.Linq;

namespace ProjectEvolution.Game;

/// <summary>
/// Analyzes tile connectivity patterns from TMX maps and creates autotiling rules
/// </summary>
public static class TileConnectivityAnalyzer
{
    public class TileAdjacency
    {
        public int TileId { get; set; }
        public HashSet<int> AdjacentNorth { get; set; } = new();
        public HashSet<int> AdjacentSouth { get; set; } = new();
        public HashSet<int> AdjacentEast { get; set; } = new();
        public HashSet<int> AdjacentWest { get; set; } = new();
        public int Frequency { get; set; }
    }

    public class TileSet
    {
        public string Name { get; set; } = "";
        public HashSet<int> TileIds { get; set; } = new();
        public int MinTileId => TileIds.Any() ? TileIds.Min() : 0;
        public int MaxTileId => TileIds.Any() ? TileIds.Max() : 0;
    }

    public static void AnalyzeTileConnectivity()
    {
        Console.Clear();
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("       TILE CONNECTIVITY ANALYZER");
        Console.WriteLine("       Understanding Which Tiles Connect");
        Console.WriteLine("═══════════════════════════════════════════════════════════\n");

        string[] tmxFiles =
        {
            "Assets/Tilesets/extracted/Map/sample_map.tmx",
            "Assets/Tilesets/extracted/Map/sample_indoor.tmx"
        };

        var allAdjacencies = new Dictionary<int, TileAdjacency>();

        foreach (var tmxPath in tmxFiles)
        {
            if (!File.Exists(tmxPath))
            {
                Console.WriteLine($"WARNING: File not found: {tmxPath}");
                continue;
            }

            Console.WriteLine($"\n--- Analyzing: {Path.GetFileName(tmxPath)} ---");
            AnalyzeTMXConnectivity(tmxPath, allAdjacencies);
        }

        Console.WriteLine("\n═══════════════════════════════════════════════════════════");
        Console.WriteLine("CONNECTIVITY ANALYSIS RESULTS");
        Console.WriteLine("═══════════════════════════════════════════════════════════\n");

        // Identify tile sets based on connectivity
        var tileSets = IdentifyTileSets(allAdjacencies);

        Console.WriteLine($"\nIdentified {tileSets.Count} Tile Sets:\n");

        foreach (var tileSet in tileSets.OrderBy(ts => ts.MinTileId))
        {
            Console.WriteLine($"  {tileSet.Name}:");
            Console.WriteLine($"    Tiles: {tileSet.MinTileId}-{tileSet.MaxTileId} ({tileSet.TileIds.Count} tiles)");
            Console.WriteLine($"    IDs: {string.Join(", ", tileSet.TileIds.OrderBy(t => t).Take(10))}{(tileSet.TileIds.Count > 10 ? "..." : "")}");

            // Show example conversions
            var firstTile = tileSet.TileIds.OrderBy(t => t).First();
            int row = (firstTile - 1) / 57;
            int col = (firstTile - 1) % 57;
            Console.WriteLine($"    Example: Tile {firstTile} (0-based: {firstTile - 1} = row {row}, col {col})");
            Console.WriteLine();
        }

        // Show detailed connectivity for common tiles
        ShowDetailedConnectivity(allAdjacencies);

        // Save to file
        SaveConnectivityReport(allAdjacencies, tileSets);

        Console.WriteLine("\n═══════════════════════════════════════════════════════════");
        Console.WriteLine("Report saved to: tile-connectivity.txt");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void AnalyzeTMXConnectivity(string tmxPath, Dictionary<int, TileAdjacency> adjacencies)
    {
        var doc = XDocument.Load(tmxPath);
        var map = doc.Element("map");
        if (map == null) return;

        int width = int.Parse(map.Attribute("width")?.Value ?? "0");
        int height = int.Parse(map.Attribute("height")?.Value ?? "0");

        // Analyze each layer
        var layers = map.Elements("layer").ToList();
        foreach (var layer in layers)
        {
            string layerName = layer.Attribute("name")?.Value ?? "Unknown";

            var data = layer.Element("data");
            if (data == null) continue;

            try
            {
                var tileIds = DecodeTMXData(data.Value);
                AnalyzeLayerConnectivity(tileIds, width, height, layerName, adjacencies);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error analyzing layer {layerName}: {ex.Message}");
            }
        }
    }

    private static void AnalyzeLayerConnectivity(int[] tiles, int width, int height, string layerName,
        Dictionary<int, TileAdjacency> adjacencies)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                int tileId = tiles[index];

                if (tileId == 0) continue; // Empty tile

                if (!adjacencies.ContainsKey(tileId))
                {
                    adjacencies[tileId] = new TileAdjacency { TileId = tileId };
                }

                adjacencies[tileId].Frequency++;

                // Check adjacent tiles
                // North
                if (y > 0)
                {
                    int northTile = tiles[(y - 1) * width + x];
                    if (northTile != 0)
                        adjacencies[tileId].AdjacentNorth.Add(northTile);
                }

                // South
                if (y < height - 1)
                {
                    int southTile = tiles[(y + 1) * width + x];
                    if (southTile != 0)
                        adjacencies[tileId].AdjacentSouth.Add(southTile);
                }

                // East
                if (x < width - 1)
                {
                    int eastTile = tiles[y * width + (x + 1)];
                    if (eastTile != 0)
                        adjacencies[tileId].AdjacentEast.Add(eastTile);
                }

                // West
                if (x > 0)
                {
                    int westTile = tiles[y * width + (x - 1)];
                    if (westTile != 0)
                        adjacencies[tileId].AdjacentWest.Add(westTile);
                }
            }
        }
    }

    private static List<TileSet> IdentifyTileSets(Dictionary<int, TileAdjacency> adjacencies)
    {
        var tileSets = new List<TileSet>();
        var processed = new HashSet<int>();

        foreach (var tileId in adjacencies.Keys.OrderBy(t => t))
        {
            if (processed.Contains(tileId)) continue;

            // Find all tiles that connect to this one
            var connectedTiles = new HashSet<int> { tileId };
            var toProcess = new Queue<int>();
            toProcess.Enqueue(tileId);

            while (toProcess.Count > 0)
            {
                var current = toProcess.Dequeue();
                var adj = adjacencies[current];

                foreach (var neighbor in adj.AdjacentNorth.Concat(adj.AdjacentSouth)
                    .Concat(adj.AdjacentEast).Concat(adj.AdjacentWest))
                {
                    if (!connectedTiles.Contains(neighbor) && !processed.Contains(neighbor))
                    {
                        connectedTiles.Add(neighbor);
                        toProcess.Enqueue(neighbor);
                    }
                }
            }

            // Check if this is a coherent tile set (tiles are sequential or clustered)
            if (connectedTiles.Count >= 3)
            {
                var sorted = connectedTiles.OrderBy(t => t).ToList();
                var min = sorted.First();
                var max = sorted.Last();

                // Determine tile set name based on range
                string name = DetermineTileSetName(min - 1, sorted); // Convert to 0-based

                tileSets.Add(new TileSet
                {
                    Name = name,
                    TileIds = connectedTiles
                });

                foreach (var tile in connectedTiles)
                    processed.Add(tile);
            }
        }

        return tileSets;
    }

    private static string DetermineTileSetName(int firstTileZeroBased, List<int> tileIds)
    {
        int row = firstTileZeroBased / 57;
        int avgCount = tileIds.Count;

        // Heuristics based on row and tile count
        if (row >= 0 && row <= 1)
            return "Floor/Ground Tiles";
        else if (row >= 2 && row <= 5)
            return "Wall/Structure Tiles";
        else if (row >= 6 && row <= 9)
            return "Nature Tiles (Trees/Rocks/Water)";
        else if (row >= 10 && row <= 15)
            return "Item/Object Tiles";
        else if (row >= 16 && row <= 26)
            return "Creature/Monster Tiles";
        else if (row >= 27)
            return "Character Tiles";
        else
            return $"Tile Set (Row {row})";
    }

    private static void ShowDetailedConnectivity(Dictionary<int, TileAdjacency> adjacencies)
    {
        Console.WriteLine("\nDETAILED CONNECTIVITY (Most Common Tiles):\n");

        var topTiles = adjacencies.Values
            .OrderByDescending(a => a.Frequency)
            .Take(10);

        foreach (var adj in topTiles)
        {
            int zeroBasedId = adj.TileId - 1;
            int row = zeroBasedId / 57;
            int col = zeroBasedId % 57;

            Console.WriteLine($"Tile {adj.TileId} (0-based: {zeroBasedId} = row {row}, col {col}) - Used {adj.Frequency} times");

            if (adj.AdjacentNorth.Any())
                Console.WriteLine($"  Connects North to: {string.Join(", ", adj.AdjacentNorth.OrderBy(t => t).Take(5))}");
            if (adj.AdjacentSouth.Any())
                Console.WriteLine($"  Connects South to: {string.Join(", ", adj.AdjacentSouth.OrderBy(t => t).Take(5))}");
            if (adj.AdjacentEast.Any())
                Console.WriteLine($"  Connects East to: {string.Join(", ", adj.AdjacentEast.OrderBy(t => t).Take(5))}");
            if (adj.AdjacentWest.Any())
                Console.WriteLine($"  Connects West to: {string.Join(", ", adj.AdjacentWest.OrderBy(t => t).Take(5))}");
            Console.WriteLine();
        }
    }

    private static void SaveConnectivityReport(Dictionary<int, TileAdjacency> adjacencies, List<TileSet> tileSets)
    {
        using var writer = new StreamWriter("tile-connectivity.txt");

        writer.WriteLine("TILE CONNECTIVITY ANALYSIS REPORT");
        writer.WriteLine("Generated: " + DateTime.Now);
        writer.WriteLine("═══════════════════════════════════════════════════════════\n");

        writer.WriteLine("TILE SETS IDENTIFIED:\n");
        foreach (var tileSet in tileSets.OrderBy(ts => ts.MinTileId))
        {
            writer.WriteLine($"{tileSet.Name}:");
            writer.WriteLine($"  Tile IDs: {string.Join(", ", tileSet.TileIds.OrderBy(t => t))}");
            writer.WriteLine($"  Range: {tileSet.MinTileId}-{tileSet.MaxTileId}");
            writer.WriteLine();
        }

        writer.WriteLine("\n═══════════════════════════════════════════════════════════");
        writer.WriteLine("DETAILED CONNECTIVITY:\n");

        foreach (var adj in adjacencies.Values.OrderBy(a => a.TileId))
        {
            int zeroBasedId = adj.TileId - 1;
            int row = zeroBasedId / 57;
            int col = zeroBasedId % 57;

            writer.WriteLine($"Tile {adj.TileId} (0-based: {zeroBasedId} = row {row}, col {col})");
            writer.WriteLine($"  Frequency: {adj.Frequency}");

            if (adj.AdjacentNorth.Any())
                writer.WriteLine($"  North: {string.Join(", ", adj.AdjacentNorth.OrderBy(t => t))}");
            if (adj.AdjacentSouth.Any())
                writer.WriteLine($"  South: {string.Join(", ", adj.AdjacentSouth.OrderBy(t => t))}");
            if (adj.AdjacentEast.Any())
                writer.WriteLine($"  East: {string.Join(", ", adj.AdjacentEast.OrderBy(t => t))}");
            if (adj.AdjacentWest.Any())
                writer.WriteLine($"  West: {string.Join(", ", adj.AdjacentWest.OrderBy(t => t))}");
            writer.WriteLine();
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
