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

        public TurnManager turnManager { get; private set; }
        public List<Enemy> Enemies { get; private set; } = new List<Enemy>();
        private bool playerCanMove = false; //  Track if player can move

        private List<(float TimeRemaining, Action Callback)> delayedActions = new List<(float, Action)>();

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
            enemyTexture = _content.Load<Texture2D>("rat");
            openExitTexture = _content.Load<Texture2D>("OpenExitTile");
            gameMap.LoadContent(_content);

            // Load maps
            string mapsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Content", "Maps");
            if (Directory.Exists(mapsFolder))
            {
                Random rand = new Random();
                premadeMapFiles = Directory.GetFiles(mapsFolder, "*.txt")
                                           .OrderBy(x => rand.Next())
                                           .ToList();
            }

            // Load first map
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

            //  Ensure map is fully loaded before spawning enemies
            if (gameMap.Rows > 0 && gameMap.Columns > 0)
            {
               // LogToFile("Spawning enemies AFTER map is confirmed loaded...");
                SpawnEnemies(3); 
            }

            turnManager.StartPlayerTurn();
        }

        public void ScheduleDelayedAction(float delay, Action action)
        {
            delayedActions.Add((delay, action));
        }

        public void Update(GameTime gameTime)
        {
            LogToFile($"Current Turn: {turnManager.CurrentTurn}"); // Debugging turn state
            LogToFile($"IsPlayerTurn() Check: {turnManager.IsPlayerTurn()}");
            LogToFile($"playerCanMove: {playerCanMove}");

            // Process scheduled actions with delays
            for (int i = delayedActions.Count - 1; i >= 0; i--)
            {
                var (timeRemaining, callback) = delayedActions[i];
                timeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (timeRemaining <= 0)
                {
                    callback.Invoke();
                    delayedActions.RemoveAt(i);
                }
                else
                {
                    delayedActions[i] = (timeRemaining, callback); // Update remaining time
                }
            }

            KeyboardState currentKeyboardState = Keyboard.GetState();

            //  Player should only move on their turn
            if (playerCanMove && turnManager.IsPlayerTurn())
            {
                LogToFile(" Player Turn Confirmed - Handling Movement...");
                HandlePlayerTurn(currentKeyboardState);
            }
            else
            {
                LogToFile(" Player input ignored - Not Player's Turn");
            }

            // Prevent exit until enemies are defeated
            if (gameMap.Tiles[playerGridPosition.Y, playerGridPosition.X] is ExitTile && Enemies.Count == 0)
            {
                LoadNextMap();
            }

            previousKeyboardState = currentKeyboardState;
        }

        public void AllowPlayerInput(bool canMove)
        {
            playerCanMove = canMove;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 mapOffset = new Vector2(
                (_graphics.PreferredBackBufferWidth - gameMap.MapWidth) / 2,
                (_graphics.PreferredBackBufferHeight - gameMap.MapHeight) / 2);
            Matrix transform = Matrix.CreateTranslation(mapOffset.X, mapOffset.Y, 0);

            spriteBatch.Begin(transformMatrix: transform, samplerState: SamplerState.PointClamp);
            gameMap.Draw(spriteBatch);

            foreach (var enemy in Enemies)
            {
                enemy.Draw(spriteBatch);
            }

            spriteBatch.Draw(playerTexture, playerPosition, null, Color.White, 0f,
                new Vector2(playerTexture.Width / 2, playerTexture.Height / 2),
                Vector2.One, SpriteEffects.None, 0f);

            spriteBatch.End();
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
            SpawnEnemies(1);
        }

        public void HandlePlayerTurn(KeyboardState currentKeyboardState)
        {
            if (!turnManager.IsPlayerTurn()) return; // Prevents movement during enemy turns

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
                        playerGridPosition.Y * gameMap.TileHeight + gameMap.TileHeight / 2
                    );

                   // LogToFile($"Player moved to {playerGridPosition}");

                    // End the turn AFTER movement
                    turnManager.EndPlayerTurn();
                }
            }
        }

        public void SetPlayerCanMove(bool canMove)
        {
            playerCanMove = canMove;
            LogToFile($"playerCanMove set to: {playerCanMove}");
        }

        private bool IsCellAccessible(int col, int row)
        {
            if (row < 0 || row >= gameMap.Rows || col < 0 || col >= gameMap.Columns)
                return false;

            Tile destinationTile = gameMap.Tiles[row, col];

            if (destinationTile is OpenExitTile) return true;
            if (destinationTile is ExitTile) return false;

            return destinationTile != null && destinationTile.Walkable;
        }

        private void ResetPlayerPosition()
        {
            int startCol = gameMap.Columns / 2;
            int startRow = gameMap.Rows / 2;
            playerGridPosition = new Point(startCol, startRow);
            playerPosition = new Vector2(
                playerGridPosition.X * gameMap.TileWidth + gameMap.TileWidth / 2,
                playerGridPosition.Y * gameMap.TileHeight + gameMap.TileHeight / 2);
        }

        private void SpawnEnemies(int count)
        {
            Random rand = new Random();
            List<Point> availableTiles = new List<Point>();

            for (int row = 0; row < gameMap.Rows; row++)
            {
                for (int col = 0; col < gameMap.Columns; col++)
                {
                    if (gameMap.IsTileWalkable(new Point(col, row)) && new Point(col, row) != playerGridPosition)
                    {
                        availableTiles.Add(new Point(col, row));
                    }
                }
            }

            for (int i = 0; i < count && availableTiles.Count > 0; i++)
            {
                int index = rand.Next(availableTiles.Count);
                Point spawnPoint = availableTiles[index];
                availableTiles.RemoveAt(index);

                Enemies.Add(new Enemy(enemyTexture, spawnPoint, gameMap, this));
               // LogToFile($"Spawned enemy at {spawnPoint}");
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
