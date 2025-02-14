using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Object_Oriented_Map_System.MapSystem;
using Object_Oriented_Map_System.MapSystem.Tiles;

namespace Object_Oriented_Map_System.Managers
{
    public class GameManager
    {
        private ContentManager _content;
        private GraphicsDeviceManager _graphics;
        private Map gameMap;
        private Texture2D playerTexture;
        private Vector2 playerPosition;
        private Point playerGridPosition;
        private KeyboardState previousKeyboardState;

        private List<string> premadeMapFiles = new List<string>();
        private int currentPremadeMapIndex = 0;
        private const int requiredRows = 10;
        private const int requiredColumns = 15;

        public GameManager(GraphicsDeviceManager graphics, ContentManager content)
        {
            _graphics = graphics;
            _content = content;
            gameMap = new Map(requiredRows, requiredColumns);
            previousKeyboardState = Keyboard.GetState();
        }

        public void LoadContent()
        {
            playerTexture = _content.Load<Texture2D>("player");
            gameMap.LoadContent(_content);

            // Locate premade maps.
            string mapsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Content", "Maps");
            if (Directory.Exists(mapsFolder))
            {
                Random rand = new Random();
                premadeMapFiles = Directory.GetFiles(mapsFolder, "*.txt")
                                           .OrderBy(x => rand.Next())
                                           .ToList();
            }

            // Load the first premade map if available; otherwise, generate a random map.
            if (premadeMapFiles.Count > 0)
            {
                LoadPremadeMap(premadeMapFiles[currentPremadeMapIndex]);
                currentPremadeMapIndex++;
            }
            else
            {
                gameMap.GenerateRandomMap();
            }

            ResetPlayerPosition();
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();

            // Handle grid-based movement (using arrow keys and WASD).
            // Up
            if ((currentKeyboardState.IsKeyDown(Keys.Up) || currentKeyboardState.IsKeyDown(Keys.W)) &&
                !(previousKeyboardState.IsKeyDown(Keys.Up) || previousKeyboardState.IsKeyDown(Keys.W)))
            {
                int newRow = playerGridPosition.Y - 1;
                if (IsCellAccessible(playerGridPosition.X, newRow))
                    playerGridPosition.Y = newRow;
            }
            // Down
            if ((currentKeyboardState.IsKeyDown(Keys.Down) || currentKeyboardState.IsKeyDown(Keys.S)) &&
                !(previousKeyboardState.IsKeyDown(Keys.Down) || previousKeyboardState.IsKeyDown(Keys.S)))
            {
                int newRow = playerGridPosition.Y + 1;
                if (IsCellAccessible(playerGridPosition.X, newRow))
                    playerGridPosition.Y = newRow;
            }
            // Left
            if ((currentKeyboardState.IsKeyDown(Keys.Left) || currentKeyboardState.IsKeyDown(Keys.A)) &&
                !(previousKeyboardState.IsKeyDown(Keys.Left) || previousKeyboardState.IsKeyDown(Keys.A)))
            {
                int newCol = playerGridPosition.X - 1;
                if (IsCellAccessible(newCol, playerGridPosition.Y))
                    playerGridPosition.X = newCol;
            }
            // Right
            if ((currentKeyboardState.IsKeyDown(Keys.Right) || currentKeyboardState.IsKeyDown(Keys.D)) &&
                !(previousKeyboardState.IsKeyDown(Keys.Right) || previousKeyboardState.IsKeyDown(Keys.D)))
            {
                int newCol = playerGridPosition.X + 1;
                if (IsCellAccessible(newCol, playerGridPosition.Y))
                    playerGridPosition.X = newCol;
            }

            // Update the player's pixel position.
            playerPosition = new Vector2(
                playerGridPosition.X * gameMap.TileWidth + gameMap.TileWidth / 2,
                playerGridPosition.Y * gameMap.TileHeight + gameMap.TileHeight / 2);

            // If the player is on an exit tile, load the next map.
            if (gameMap.Tiles[playerGridPosition.Y, playerGridPosition.X] is ExitTile)
            {
                if (currentPremadeMapIndex < premadeMapFiles.Count)
                {
                    LoadPremadeMap(premadeMapFiles[currentPremadeMapIndex]);
                    currentPremadeMapIndex++;
                }
                else
                {
                    gameMap.GenerateRandomMap();
                    ResetPlayerPosition();
                }
            }

            previousKeyboardState = currentKeyboardState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Calculate an offset to center the map.
            Vector2 mapOffset = new Vector2(
                (_graphics.PreferredBackBufferWidth - gameMap.MapWidth) / 2,
                (_graphics.PreferredBackBufferHeight - gameMap.MapHeight) / 2);
            Matrix transform = Matrix.CreateTranslation(mapOffset.X, mapOffset.Y, 0);

            spriteBatch.Begin(transformMatrix: transform, samplerState: SamplerState.PointClamp);
            gameMap.Draw(spriteBatch);
            spriteBatch.Draw(playerTexture, playerPosition, null, Color.White, 0f,
                new Vector2(playerTexture.Width / 2, playerTexture.Height / 2),
                Vector2.One, SpriteEffects.None, 0f);
            spriteBatch.End();
        }

        private bool IsCellAccessible(int col, int row)
        {
            if (row < 0 || row >= gameMap.Rows || col < 0 || col >= gameMap.Columns)
                return false;
            Tile destinationTile = gameMap.Tiles[row, col];
            return destinationTile != null && (destinationTile.Walkable || destinationTile is ExitTile);
        }

        private void ResetPlayerPosition()
        {
            int startCol = gameMap.Columns / 2;
            int startRow = gameMap.Rows / 2;
            if (startCol <= 0) startCol = 1;
            if (startRow <= 0) startRow = 1;
            if (startCol >= gameMap.Columns - 1) startCol = gameMap.Columns - 2;
            if (startRow >= gameMap.Rows - 1) startRow = gameMap.Rows - 2;
            playerGridPosition = new Point(startCol, startRow);
            playerPosition = new Vector2(
                playerGridPosition.X * gameMap.TileWidth + gameMap.TileWidth / 2,
                playerGridPosition.Y * gameMap.TileHeight + gameMap.TileHeight / 2);
        }

        private void LoadPremadeMap(string mapFilePath)
        {
            string[] lines = File.ReadAllLines(mapFilePath);
            if (lines.Length == requiredRows && lines[0].Length == requiredColumns)
                gameMap.LoadMapFromFile(mapFilePath);
            else
                gameMap.GenerateRandomMap();

            ResetPlayerPosition();
        }
    }
}