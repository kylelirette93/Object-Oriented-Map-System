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
using Object_Oriented_Map_System.Entities; 
using Microsoft.Xna.Framework.Audio;

namespace Object_Oriented_Map_System.Managers
{
    public class GameManager
    {
        private ContentManager _content;
        private GraphicsDeviceManager _graphics;
        private Map gameMap;
        private Texture2D playerTexture;
        private Texture2D enemyTexture;
        private Texture2D openExitTexture;
        private Vector2 playerPosition;
        private Point playerGridPosition;
        private KeyboardState previousKeyboardState;

        private List<string> premadeMapFiles = new List<string>();
        private int currentPremadeMapIndex = 0;
        private const int requiredRows = 10;
        private const int requiredColumns = 15;

        private TurnManager turnManager;
        public List<Enemy> Enemies { get; private set; } = new List<Enemy>();

        public Point PlayerGridPosition => playerGridPosition;

        public GameManager(GraphicsDeviceManager graphics, ContentManager content)
        {
            _graphics = graphics;
            _content = content;
            gameMap = new Map(requiredRows, requiredColumns);
            previousKeyboardState = Keyboard.GetState();
            turnManager = new TurnManager(this);
        }

        public void LoadContent()
        {
            playerTexture = _content.Load<Texture2D>("player");
            enemyTexture = _content.Load<Texture2D>("rat"); // Load enemy texture
            openExitTexture = _content.Load<Texture2D>("OpenExitTile");
            gameMap.LoadContent(_content);
            turnManager.StartPlayerTurn();

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
            SpawnEnemies(0); // Spawn 3 enemies
        }

        private void LoadPremadeMap(string mapFilePath)
        {
            if (File.Exists(mapFilePath))
            {
                gameMap.LoadMapFromFile(mapFilePath);
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
            turnManager.Update(gameTime);

            // Player movement logic
            HandlePlayerTurn(currentKeyboardState);

            // Prevent map exit until all enemies are defeated
            if (gameMap.Tiles[playerGridPosition.Y, playerGridPosition.X] is ExitTile)
            {
                if (Enemies.Count == 0)
                {
                    LoadNextMap();
                }
                else
                {
                    Console.WriteLine("Defeat all enemies before leaving!");
                }
            }

            previousKeyboardState = currentKeyboardState;

            CheckExitTile();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 mapOffset = new Vector2(
                (_graphics.PreferredBackBufferWidth - gameMap.MapWidth) / 2,
                (_graphics.PreferredBackBufferHeight - gameMap.MapHeight) / 2);
            Matrix transform = Matrix.CreateTranslation(mapOffset.X, mapOffset.Y, 0);

            spriteBatch.Begin(transformMatrix: transform, samplerState: SamplerState.PointClamp);
            gameMap.Draw(spriteBatch);

            // Draw enemies
            foreach (var enemy in Enemies)
            {
                enemy.Draw(spriteBatch);
            }

            // Draw player
            spriteBatch.Draw(playerTexture, playerPosition, null, Color.White, 0f,
                new Vector2(playerTexture.Width / 2, playerTexture.Height / 2),
                Vector2.One, SpriteEffects.None, 0f);

            spriteBatch.End();
        }

        private void CheckExitTile()
        {
            // If all enemies are defeated, change the exit tile
            if (Enemies.Count == 0)
            {
                ReplaceExitTile();
            }
        }

        private void ReplaceExitTile()
        {
            for (int row = 0; row < gameMap.Rows; row++)
            {
                for (int col = 0; col < gameMap.Columns; col++)
                {
                    if (gameMap.Tiles[row, col] is ExitTile)
                    {
                        Vector2 tilePosition = gameMap.Tiles[row, col].Position;
                        
                        gameMap.Tiles[row, col] = new OpenExitTile(gameMap.openExitTexture, tilePosition);
                    }
                }
            }
        }

        public void HandlePlayerTurn(KeyboardState currentKeyboardState)
        {
            if (turnManager.IsPlayerTurn())
            {
                Point targetPosition = playerGridPosition;

                if (currentKeyboardState.IsKeyDown(Keys.Up) && !previousKeyboardState.IsKeyDown(Keys.Up))
                    targetPosition.Y -= 1;
                if (currentKeyboardState.IsKeyDown(Keys.Down) && !previousKeyboardState.IsKeyDown(Keys.Down))
                    targetPosition.Y += 1;
                if (currentKeyboardState.IsKeyDown(Keys.Left) && !previousKeyboardState.IsKeyDown(Keys.Left))
                    targetPosition.X -= 1;
                if (currentKeyboardState.IsKeyDown(Keys.Right) && !previousKeyboardState.IsKeyDown(Keys.Right))
                    targetPosition.X += 1;

                if (targetPosition != playerGridPosition)
                {
                    if (IsCellAccessible(targetPosition.X, targetPosition.Y))
                    {
                        playerGridPosition = targetPosition;
                        playerPosition = new Vector2(
                            playerGridPosition.X * gameMap.TileWidth + gameMap.TileWidth / 2,
                            playerGridPosition.Y * gameMap.TileHeight + gameMap.TileHeight / 2);
                    }

                    if (PlayerTurnEnded())
                    {
                        turnManager.StartEnemyTurn();
                    }
                }
            }
        }

        private bool IsCellAccessible(int col, int row)
        {
            if (row < 0 || row >= gameMap.Rows || col < 0 || col >= gameMap.Columns)
                return false;

            Tile destinationTile = gameMap.Tiles[row, col];

            // Allow movement onto OpenExitTile
            if (destinationTile is OpenExitTile)
            {
                return true;
            }

            // Block movement into ExitTile (until replaced)
            if (destinationTile is ExitTile)
            {
                return false;
            }

            return destinationTile != null && destinationTile.Walkable;
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

        private void LoadNextMap()
        {
            if (currentPremadeMapIndex < premadeMapFiles.Count)
            {
                LoadPremadeMap(premadeMapFiles[currentPremadeMapIndex]);
                currentPremadeMapIndex++;
            }
            else
            {
                gameMap.GenerateRandomMap();
            }

            ResetPlayerPosition();
            Enemies.Clear();
            SpawnEnemies(3);
        }

        private void SpawnEnemies(int count)
        {
            Random rand = new Random();
            for (int i = 0; i < count; i++)
            {
                Point spawnPoint;
                do
                {
                    spawnPoint = new Point(rand.Next(1, gameMap.Columns - 1), rand.Next(1, gameMap.Rows - 1));
                } while (!gameMap.IsTileWalkable(spawnPoint) || spawnPoint == playerGridPosition);

                Enemies.Add(new Enemy(enemyTexture, spawnPoint, gameMap, this));
            }
        }

        private bool PlayerTurnEnded()
        {
            return true; // Placeholder logic, modify as needed
        }
    }
}