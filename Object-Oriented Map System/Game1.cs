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

            // Decide on 1 or 2 exit tiles
            int exitCount = rand.Next(1, 3); // Returns 1 or 2

            // Create a list of border positions (using row, col coordinates)
            List<Point> borderPositions = new List<Point>();
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    if (row == 0 || row == Rows - 1 || col == 0 || col == Columns - 1)
                    {
                        borderPositions.Add(new Point(row, col));
                    }
                }
            }

            // Randomly select exit positions from the border positions.
            List<Point> exitPositions = new List<Point>();
            for (int i = 0; i < exitCount && borderPositions.Count > 0; i++)
            {
                int index = rand.Next(borderPositions.Count);
                exitPositions.Add(borderPositions[index]);
                borderPositions.RemoveAt(index);
            }

            // Now fill the map.
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    // Calculate the tile's position in pixels
                    Vector2 position = new Vector2(col * tileWidth, row * tileHeight);

                    // Check if the tile is on the border.
                    if (row == 0 || row == Rows - 1 || col == 0 || col == Columns - 1)
                    {
                        // If it's one of the chosen exit positions, set it as an exit tile.
                        if (exitPositions.Exists(p => p.X == row && p.Y == col))
                        {
                            Tiles[row, col] = new ExitTile(exitTexture, position);
                        }
                        else
                        {
                            // Otherwise, it's a non-walkable border tile.
                            Tiles[row, col] = new NonWalkableTile(nonWalkableTexture, position);
                        }
                    }
                    else
                    {
                        // All inner tiles are walkable.
                        Tiles[row, col] = new WalkableTile(walkableTexture, position);
                    }
                }
            }
        }

        /// <summary>
        /// Loads a predefined map from a text file.
        /// Example file content (each character represents a tile):
        ///   . = walkable, # = non-walkable, E = exit
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

                    if (c == '.')
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

            // For this sprint, generate a random map.
            // In a future sprint you can switch to gameMap.LoadMapFromFile("YourMapFile.txt");
            gameMap.GenerateRandomMap();
        }

        protected override void Update(GameTime gameTime)
        {
            // Exit if needed.
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
          
            KeyboardState currentKeyboardState = Keyboard.GetState();

            // Only move one grid cell per key press:
            if (currentKeyboardState.IsKeyDown(Keys.Up) && !previousKeyboardState.IsKeyDown(Keys.Up))
            {
                if (playerGridPosition.Y > 1)  // avoid border row 0
                    playerGridPosition.Y--;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Down) && !previousKeyboardState.IsKeyDown(Keys.Down))
            {
                if (playerGridPosition.Y < gameMap.Rows - 2) // avoid last border row
                    playerGridPosition.Y++;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Left) && !previousKeyboardState.IsKeyDown(Keys.Left))
            {
                if (playerGridPosition.X > 1)  // avoid border column 0
                    playerGridPosition.X--;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Right) && !previousKeyboardState.IsKeyDown(Keys.Right))
            {
                if (playerGridPosition.X < gameMap.Columns - 2) // avoid last border column
                    playerGridPosition.X++;
            }

            // Update the player's pixel position using the map's tile dimensions.
            playerPosition = new Vector2(
                playerGridPosition.X * gameMap.TileWidth + gameMap.TileWidth / 2,
                playerGridPosition.Y * gameMap.TileHeight + gameMap.TileHeight / 2);

            previousKeyboardState = currentKeyboardState;

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
    }
}
