using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.IO;
using System.Collections.Generic;
using Object_Oriented_Map_System.MapSystem.Tiles;

namespace Object_Oriented_Map_System.MapSystem
{
    public class Map
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public Tile[,] Tiles { get; private set; }

        // Tile dimensions (assumed fixed for simplicity)
        private int tileWidth = 32;
        private int tileHeight = 32;

        public int TileWidth { get { return tileWidth; } }
        public int TileHeight { get { return tileHeight; } }
        public int MapWidth { get { return Columns * tileWidth; } }
        public int MapHeight { get { return Rows * tileHeight; } }

        // Textures for tile types
        private Texture2D walkableTexture;
        private Texture2D nonWalkableTexture;
        private Texture2D exitTexture;
        public Texture2D openExitTexture;

        public Point SpawnPoint { get; private set; }

        public Map(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            Tiles = new Tile[rows, columns];
        }

        public void LoadContent(ContentManager content)
        {
            walkableTexture = content.Load<Texture2D>("walkTile");
            nonWalkableTexture = content.Load<Texture2D>("wallTile");
            exitTexture = content.Load<Texture2D>("exitTile");
            openExitTexture = content.Load<Texture2D>("OpenExitTile");

            tileWidth = walkableTexture.Width;
            tileHeight = walkableTexture.Height;
        }

        public void GenerateRandomMap()
        {
            Random rand = new Random();

            int walkableTileCount = 0;

            // Decide on 2 to 4 exit tiles.
            int exitCount = rand.Next(2, 5);

            List<Point> exitPositions = new List<Point>();
            List<int> selectedWalls = new List<int>();
            while (selectedWalls.Count < exitCount)
            {
                int wall = rand.Next(0, 4); // 0 = top, 1 = right, 2 = bottom, 3 = left
                if (!selectedWalls.Contains(wall))
                    selectedWalls.Add(wall);
            }

            // For each selected wall, choose a random exit position (avoiding corners).
            foreach (int wall in selectedWalls)
            {
                switch (wall)
                {
                    case 0: // Top wall: row = 0, valid columns: 1 to Columns-2
                        int topCol = rand.Next(1, Columns - 1);
                        exitPositions.Add(new Point(topCol, 0));
                        break;
                    case 1: // Right wall: col = Columns - 1, valid rows: 1 to Rows-2
                        int rightRow = rand.Next(1, Rows - 1);
                        exitPositions.Add(new Point(Columns - 1, rightRow));
                        break;
                    case 2: // Bottom wall: row = Rows - 1, valid columns: 1 to Columns-2
                        int bottomCol = rand.Next(1, Columns - 1);
                        exitPositions.Add(new Point(bottomCol, Rows - 1));
                        break;
                    case 3: // Left wall: col = 0, valid rows: 1 to Rows-2
                        int leftRow = rand.Next(1, Rows - 1);
                        exitPositions.Add(new Point(0, leftRow));
                        break;
                }
            }

            // Fill the map with tiles.
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    Vector2 position = new Vector2(col * tileWidth, row * tileHeight);
                    if (row == 0 || row == Rows - 1 || col == 0 || col == Columns - 1)
                    {
                        if (exitPositions.Exists(p => p.X == col && p.Y == row))
                            Tiles[row, col] = new ExitTile(exitTexture, position);
                        else
                            Tiles[row, col] = new NonWalkableTile(nonWalkableTexture, position);
                    }
                    else
                    {
                        Tiles[row, col] = new WalkableTile(walkableTexture, position);
                        walkableTileCount++;
                    }
                }
            }

            // Find a valid walkable tile for the spawn point AFTER map is generated
            List<Point> walkableTiles = new List<Point>();
            for (int row = 1; row < Rows - 1; row++)
            {
                for (int col = 1; col < Columns - 1; col++)
                {
                    if (Tiles[row, col] is WalkableTile)
                        walkableTiles.Add(new Point(col, row));
                }
            }

            if (walkableTiles.Count > 0)
            {
                SpawnPoint = walkableTiles[rand.Next(walkableTiles.Count)];
            }

            GenerateObstacles(exitPositions);

            // Ensure a valid path exists between the spawn point and at least one exit
            if (!ValidatePathExists())
            {
                LogToFile("Invalid map generated! Retrying full map generation...");
                GenerateRandomMap(); // Regenerate the entire map if it's invalid
                return;
            }
        }

        private void GenerateObstacles(List<Point> exitPositions)
        {
            Random rand = new Random();
            List<Rectangle> placedObstacles = new List<Rectangle>(); // Track placed obstacles
            int numClusters = rand.Next(3, 6); // Randomize number of obstacle clusters
            int attempts = 0;

            for (int i = 0; i < numClusters; i++)
            {
                if (attempts > 100) break; // Prevent infinite loops if placement fails

                int width = rand.Next(2, 5); // Cluster width: 2 to 4 tiles
                int height = rand.Next(2, 5); // Cluster height: 2 to 4 tiles

                int x = 0, y = 0;
                bool validPlacement;
                Rectangle newCluster = new Rectangle(); // Declare it outside loop

                do
                {
                    x = rand.Next(1, Columns - width - 1);
                    y = rand.Next(1, Rows - height - 1);
                    newCluster = new Rectangle(x, y, width, height); // Assign after getting x, y

                    validPlacement = true;

                    // **Ensure no obstacles touch each other**
                    foreach (Rectangle existing in placedObstacles)
                    {
                        if (newCluster.Intersects(existing) ||
                            newCluster.Intersects(new Rectangle(existing.X - 1, existing.Y - 1, existing.Width + 2, existing.Height + 2)))
                        {
                            validPlacement = false;
                            break;
                        }
                    }

                    // **Ensure obstacle does NOT cover spawn or exits**
                    if (validPlacement && (newCluster.Contains(SpawnPoint) || exitPositions.Exists(p => newCluster.Contains(p))))
                    {
                        validPlacement = false;
                    }

                    // **Ensure obstacle does NOT spawn ADJACENT to ExitTiles**
                    foreach (Point exit in exitPositions)
                    {
                        Rectangle exitArea = new Rectangle(exit.X - 1, exit.Y - 1, 3, 3); // Area 1 tile around exit
                        if (newCluster.Intersects(exitArea))
                        {
                            validPlacement = false;
                            break;
                        }
                    }

                    attempts++;

                } while (!validPlacement && attempts < 100);

                // If valid, place obstacle and track it
                if (validPlacement)
                {
                    for (int row = y; row < y + height; row++)
                    {
                        for (int col = x; col < x + width; col++)
                        {
                            Tiles[row, col] = new NonWalkableTile(nonWalkableTexture, new Vector2(col * tileWidth, row * tileHeight));
                        }
                    }

                    placedObstacles.Add(newCluster); // Now newCluster exists in this scope
                }
            }

            LogToFile($"Obstacle clusters placed: {placedObstacles.Count}");
        }

        private bool ValidatePathExists()
        {
            HashSet<Point> visited = new HashSet<Point>();
            Queue<Point> queue = new Queue<Point>();

            queue.Enqueue(SpawnPoint);
            visited.Add(SpawnPoint);

            while (queue.Count > 0)
            {
                Point current = queue.Dequeue();

                // If we reach any exit position, the path is valid
                if (Tiles[current.Y, current.X] is ExitTile)
                    return true;

                foreach (var neighbor in GetWalkableNeighbors(current))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return false; // No valid path found
        }

        private bool IsClusterPlacementValid(int startX, int startY, int width, int height)
        {
            for (int x = startX; x < startX + width; x++)
            {
                for (int y = startY; y < startY + height; y++)
                {
                    if (Tiles[y, x] is ExitTile)
                    {
                        return false; // Don't block exits
                    }
                }
            }

            return ValidatePathExists(); // Ensure the path from spawn to exit still exists
        }

        private int CountWalkableTiles()
        {
            int count = 0;
            foreach (var tile in Tiles)
            {
                if (tile is WalkableTile || tile is ExitTile)
                    count++;
            }
            return count;
        }

        private List<Point> GetWalkableNeighbors(Point current)
        {
            List<Point> neighbors = new List<Point>();

            Point[] directions = new Point[]
            {
                new Point(0, -1), // Up
                new Point(1, 0),  // Right
                new Point(0, 1),  // Down
                new Point(-1, 0)  // Left
            };

            foreach (var dir in directions)
            {
                Point neighbor = new Point(current.X + dir.X, current.Y + dir.Y);
                if (IsTileWalkable(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        public void LoadMapFromFile(string filename)
        {
            LogToFile($"Loading pre-made map: {filename}");

            if (!File.Exists(filename))
            {
                LogToFile($"ERROR: Map file not found: {filename}");
                return;
            }

            string[] lines = File.ReadAllLines(filename);
            Rows = lines.Length;
            Columns = lines[0].Length;
            Tiles = new Tile[Rows, Columns]; // Ensure it's properly initialized

            int walkableTileCount = 0;

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    Vector2 position = new Vector2(col * tileWidth, row * tileHeight);
                    char c = lines[row][col];

                    switch (c)
                    {
                        case 'w':
                            Tiles[row, col] = new WalkableTile(walkableTexture, position);
                            walkableTileCount++;
                            break;
                        case '#':
                            Tiles[row, col] = new NonWalkableTile(nonWalkableTexture, position);
                            break;
                        case 'E':
                            Tiles[row, col] = new ExitTile(exitTexture, position);
                            break;
                        default:
                            LogToFile($"Unrecognized tile character '{c}' at ({col}, {row}). Defaulting to wall.");
                            Tiles[row, col] = new NonWalkableTile(nonWalkableTexture, position);
                            break;
                    }
                }
            }

            LogToFile($"Finished loading map: {filename}");
            LogToFile($"Total Walkable Tiles: {walkableTileCount}");

            if (walkableTileCount == 0)
            {
                LogToFile("ERROR: No walkable tiles found in map!");
            }
        }

        public bool IsTileWalkable(Point gridPosition)
        {
            if (gridPosition.X < 0 || gridPosition.Y < 0 ||
                gridPosition.X >= Columns || gridPosition.Y >= Rows)
            {
                LogToFile($"Tile ({gridPosition.X}, {gridPosition.Y}) is OUT OF BOUNDS.");
                return false;
            }

            if (Tiles == null)
            {
                LogToFile("ERROR: Tiles array is NULL when checking IsTileWalkable.");
                return false;
            }

            if (Tiles[gridPosition.Y, gridPosition.X] == null)
            {
                LogToFile($"ERROR: Tile at ({gridPosition.X}, {gridPosition.Y}) is NULL.");
                return false;
            }

            return Tiles[gridPosition.Y, gridPosition.X] is WalkableTile || Tiles[gridPosition.Y, gridPosition.X] is ExitTile;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    Tiles[row, col]?.Draw(spriteBatch);
                }
            }
        }


        private void LogToFile(string message)
        {
            string logPath = "debug_log.txt";
            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
    }
}