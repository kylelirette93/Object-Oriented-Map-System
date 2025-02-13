using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using Point = Microsoft.Xna.Framework.Point;
using Color = Microsoft.Xna.Framework.Color;

namespace Object_Oriented_Map_System
{
    // -------------------------------------------
    // Base class for Tiles (Abstraction)
    // -------------------------------------------
    public abstract class Tile
    {
        public Vector2 Position { get; protected set; }
        public Texture2D Texture { get; protected set; }
        public bool Walkable { get; protected set; }

        public Tile(Texture2D texture, Vector2 position)
        {
            Texture = texture;
            Position = position;
        }

        public abstract void Draw(SpriteBatch spriteBatch);
    }

    // -------------------------------------------
    // Derived Tile classes (Inheritance & Polymorphism)
    // -------------------------------------------
    public class WalkableTile : Tile
    {
        public WalkableTile(Texture2D texture, Vector2 position)
            : base(texture, position)
        {
            Walkable = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw normally
            spriteBatch.Draw(Texture, Position, Color.White);
        }
    }

    public class NonWalkableTile : Tile
    {
        public NonWalkableTile(Texture2D texture, Vector2 position)
            : base(texture, position)
        {
            Walkable = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // For example, tint non-walkable tiles a bit darker
            spriteBatch.Draw(Texture, Position, Color.DarkGray);
        }
    }

    public class ExitTile : Tile
    {
        public ExitTile(Texture2D texture, Vector2 position)
            : base(texture, position)
        {
            // Exit tiles might be walkable but trigger a level change.
            Walkable = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Tint exit tiles green
            spriteBatch.Draw(Texture, Position, Color.Green);
        }
    }

    // -------------------------------------------
    // Map class to encapsulate the tile grid
    // -------------------------------------------
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

        // Textures for each tile type
        private Texture2D walkableTexture;
        private Texture2D nonWalkableTexture;
        private Texture2D exitTexture;

        public Map(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            Tiles = new Tile[rows, columns];
        }

        /// <summary>
        /// Load textures for the tiles.
        /// </summary>
        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            walkableTexture = content.Load<Texture2D>("walkTile");
            nonWalkableTexture = content.Load<Texture2D>("wallTile");
            exitTexture = content.Load<Texture2D>("exitTile");

            tileWidth = walkableTexture.Width;
            tileHeight = walkableTexture.Height;
        }

        /// <summary>
        /// Generates a random map using a simple random algorithm.
        /// </summary>
        public void GenerateRandomMap()
        {
            Random rand = new Random();

            // Decide on 2 - 4 exit tiles
            int exitCount = rand.Next(2, 5); // Returns 2, 3 or 4

            // List to store selected exit positions (grid coordinates: X=col, Y=row)
            List<Point> exitPositions = new List<Point>();

            // We'll use wall indices: 0 = top, 1 = right, 2 = bottom, 3 = left.
            List<int> selectedWalls = new List<int>();
            while (selectedWalls.Count < exitCount)
            {
                int wall = rand.Next(0, 4); // returns 0,1,2, or 3
                if (!selectedWalls.Contains(wall))
                    selectedWalls.Add(wall);
            }

            // For each selected wall, choose a random exit position.
            foreach (int wall in selectedWalls)
            {
                switch (wall)
                {
                    case 0: // Top wall: row = 0, valid columns are 1 to Columns-2 (avoiding corners)
                        int topCol = rand.Next(1, Columns - 1);
                        // Ensure we don't get a corner (if Columns-1 is returned, subtract one)
                        if (topCol == Columns - 1) topCol = Columns - 2;
                        exitPositions.Add(new Point(topCol, 0));
                        break;
                    case 1: // Right wall: col = Columns - 1, valid rows are 1 to Rows-2
                        int rightRow = rand.Next(1, Rows - 1);
                        if (rightRow == Rows - 1) rightRow = Rows - 2;
                        exitPositions.Add(new Point(Columns - 1, rightRow));
                        break;
                    case 2: // Bottom wall: row = Rows - 1, valid columns are 1 to Columns-2
                        int bottomCol = rand.Next(1, Columns - 1);
                        if (bottomCol == Columns - 1) bottomCol = Columns - 2;
                        exitPositions.Add(new Point(bottomCol, Rows - 1));
                        break;
                    case 3: // Left wall: col = 0, valid rows are 1 to Rows-2
                        int leftRow = rand.Next(1, Rows - 1);
                        if (leftRow == Rows - 1) leftRow = Rows - 2;
                        exitPositions.Add(new Point(0, leftRow));
                        break;
                }
            }

            // Now fill the map.
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    // Calculate the tile's pixel position.
                    Vector2 position = new Vector2(col * tileWidth, row * tileHeight);

                    // Check if the tile is on the border.
                    if (row == 0 || row == Rows - 1 || col == 0 || col == Columns - 1)
                    {
                        // If this border tile is one of the pre-selected exit positions, place an exit tile.
                        // Remember: exitPositions stores (X=col, Y=row).
                        if (exitPositions.Exists(p => p.X == col && p.Y == row))
                            Tiles[row, col] = new ExitTile(exitTexture, position);
                        else
                            Tiles[row, col] = new NonWalkableTile(nonWalkableTexture, position);
                    }
                    else
                    {
                        // Inner tiles are walkable.
                        Tiles[row, col] = new WalkableTile(walkableTexture, position);
                    }
                }
            }
        }

        /// <summary>
        /// Loads a predefined map from a text file.
        /// Example file content (each character represents a tile):
        ///   w = walkable, # = non-walkable, E = exit
        /// </summary>
        public void LoadMapFromFile(string filename)
        {
            string[] lines = System.IO.File.ReadAllLines(filename);
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
                    {
                        Tiles[row, col] = new WalkableTile(walkableTexture, position);
                    }
                    else if (c == '#')
                    {
                        Tiles[row, col] = new NonWalkableTile(nonWalkableTexture, position);
                    }
                    else if (c == 'E')
                    {
                        Tiles[row, col] = new ExitTile(exitTexture, position);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the entire map.
        /// </summary>
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

    // -------------------------------------------
    // Main Game1 class
    // -------------------------------------------
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Player fields
        Texture2D playerTexture;
        Vector2 playerPosition;
        //float playerSpeed;

        private Point playerGridPosition;
        private KeyboardState previousKeyboardState;

        // Map instance
        private Map gameMap;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Create a map with at least 10 rows x 15 columns.
            gameMap = new Map(10, 15);

            int startCol = gameMap.Columns / 2;
            int startRow = gameMap.Rows / 2;
            if (startCol == 0) startCol = 1;
            if (startRow == 0) startRow = 1;
            if (startCol >= gameMap.Columns - 1) startCol = gameMap.Columns - 2;
            if (startRow >= gameMap.Rows - 1) startRow = gameMap.Rows - 2;

            playerGridPosition = new Point(startCol, startRow);

            // Calculate the player's pixel position relative to the map.
            playerPosition = new Vector2(
                playerGridPosition.X * gameMap.TileWidth + gameMap.TileWidth / 2,
                playerGridPosition.Y * gameMap.TileHeight + gameMap.TileHeight / 2);

            // Initialize previousKeyboardState
            previousKeyboardState = Keyboard.GetState();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load player texture.
            playerTexture = Content.Load<Texture2D>("player");

            // Load tile textures for the map.
            gameMap.LoadContent(Content);

            gameMap.GenerateRandomMap();
        }

        private bool IsCellAccessible(int col, int row)
        {
            // Ensure the coordinates are within bounds.
            if (row < 0 || row >= gameMap.Rows || col < 0 || col >= gameMap.Columns)
                return false;

            Tile destinationTile = gameMap.Tiles[row, col];
            // Allow movement if the tile is walkable or an exit tile.
            return destinationTile != null && (destinationTile.Walkable || destinationTile is ExitTile);
        }

        protected override void Update(GameTime gameTime)
        {
            // Exit if needed.
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
          
            KeyboardState currentKeyboardState = Keyboard.GetState();

            // Move Up
            if ((currentKeyboardState.IsKeyDown(Keys.Up) || currentKeyboardState.IsKeyDown(Keys.W)) &&
                !(previousKeyboardState.IsKeyDown(Keys.Up) || previousKeyboardState.IsKeyDown(Keys.W)))
            {
                int newRow = playerGridPosition.Y - 1;
                if (IsCellAccessible(playerGridPosition.X, newRow))
                {
                    playerGridPosition.Y = newRow;
                }
            }

            // Move Down
            if ((currentKeyboardState.IsKeyDown(Keys.Down) || currentKeyboardState.IsKeyDown(Keys.S)) &&
                !(previousKeyboardState.IsKeyDown(Keys.Down) || previousKeyboardState.IsKeyDown(Keys.S)))
            {
                int newRow = playerGridPosition.Y + 1;
                if (IsCellAccessible(playerGridPosition.X, newRow))
                {
                    playerGridPosition.Y = newRow;
                }
            }

            // Move Left
            if ((currentKeyboardState.IsKeyDown(Keys.Left) || currentKeyboardState.IsKeyDown(Keys.A)) &&
                !(previousKeyboardState.IsKeyDown(Keys.Left) || previousKeyboardState.IsKeyDown(Keys.A)))
            {
                int newCol = playerGridPosition.X - 1;
                if (IsCellAccessible(newCol, playerGridPosition.Y))
                {
                    playerGridPosition.X = newCol;
                }
            }

            // Move Right
            if ((currentKeyboardState.IsKeyDown(Keys.Right) || currentKeyboardState.IsKeyDown(Keys.D)) &&
                !(previousKeyboardState.IsKeyDown(Keys.Right) || previousKeyboardState.IsKeyDown(Keys.D)))
            {
                int newCol = playerGridPosition.X + 1;
                if (IsCellAccessible(newCol, playerGridPosition.Y))
                {
                    playerGridPosition.X = newCol;
                }
            }

            // Update the player's pixel position using the map's tile dimensions.
            playerPosition = new Vector2(
                playerGridPosition.X * gameMap.TileWidth + gameMap.TileWidth / 2,
                playerGridPosition.Y * gameMap.TileHeight + gameMap.TileHeight / 2);

            previousKeyboardState = currentKeyboardState;

            //Check if the Player's current tile is an exit tile
            if (gameMap.Tiles[playerGridPosition.Y, playerGridPosition.X] is ExitTile)
            {
                //The player has moved into an ExitTile.
                //Load a new map (Pre-made if available, random otherwise)
                LoadNewMap();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Calculate the offset to center the map on the screen.
            Vector2 mapOffset = new Vector2(
                (_graphics.PreferredBackBufferWidth - gameMap.MapWidth) / 2,
                (_graphics.PreferredBackBufferHeight - gameMap.MapHeight) / 2);

            // Begin sprite batch with a transformation matrix to apply the offset.
            _spriteBatch.Begin(
                transformMatrix: Matrix.CreateTranslation(mapOffset.X, mapOffset.Y, 0),
                samplerState: SamplerState.PointClamp);

            gameMap.Draw(_spriteBatch);
          
            // Draw the player using the grid-based pixel position.
            _spriteBatch.Draw(playerTexture, playerPosition, null,
                Color.White, 0f, new Vector2(playerTexture.Width / 2, playerTexture.Height / 2),
                       Vector2.One, SpriteEffects.None, 0f);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void LoadNewMap()
        {
            // Define the path and required dimensions for a pre-made map.
            string mapFilePath = "Content/Maps/premadeMap.txt";
            int requiredRows = 10;
            int requiredColumns = 15;

            // Check if the premade map file exists.
            if (System.IO.File.Exists(mapFilePath))
            {
                string[] mapLines = System.IO.File.ReadAllLines(mapFilePath);
                if (mapLines.Length == requiredRows && mapLines[0].Length == requiredColumns)
                {
                    // Load the pre-made map.
                    gameMap.LoadMapFromFile(mapFilePath);
                }
                else
                {
                    // File exists but dimensions are off; fall back to random generation.
                    gameMap.GenerateRandomMap();
                }
            }
            else
            {
                // No pre-made file found; generate a random map.
                gameMap.GenerateRandomMap();
            }

            // Reset the player's grid position to a safe starting cell.
            // For example, use the center of the inner area (avoiding the border).
            int startCol = gameMap.Columns / 2;
            int startRow = gameMap.Rows / 2;
            if (startCol == 0) startCol = 1;
            if (startRow == 0) startRow = 1;
            if (startCol >= gameMap.Columns - 1) startCol = gameMap.Columns - 2;
            if (startRow >= gameMap.Rows - 1) startRow = gameMap.Rows - 2;

            playerGridPosition = new Point(startCol, startRow);
            playerPosition = new Vector2(
                playerGridPosition.X * gameMap.TileWidth + gameMap.TileWidth / 2,
                playerGridPosition.Y * gameMap.TileHeight + gameMap.TileHeight / 2);
        }
    }
}
