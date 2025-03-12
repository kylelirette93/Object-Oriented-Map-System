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
                    }
                }
            }
        }

        public void LoadMapFromFile(string filename)
        {
            string[] lines = File.ReadAllLines(filename);
            Rows = lines.Length;
            Columns = lines[0].Length;
            Tiles = new Tile[Rows, Columns];

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    Vector2 position = new Vector2(col * tileWidth, row * tileHeight);
                    char c = lines[row][col];

                    if (c == 'w')
                        Tiles[row, col] = new WalkableTile(walkableTexture, position);
                    else if (c == '#')
                        Tiles[row, col] = new NonWalkableTile(nonWalkableTexture, position);
                    else if (c == 'E')
                        Tiles[row, col] = new ExitTile(exitTexture, position);
                }
            }
        }

        public bool IsTileWalkable(Point gridPosition)
        {
            if (gridPosition.X < 0 || gridPosition.Y < 0 ||
                gridPosition.X >= Columns || gridPosition.Y >= Rows)
            {
                return false;
            }

            return Tiles[gridPosition.Y, gridPosition.X] is WalkableTile;
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
    }
}