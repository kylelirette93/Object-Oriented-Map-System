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

        private SpriteFont damageFont;
        private List<DamageText> damageTexts = new List<DamageText>();

        private List<string> premadeMapFiles = new List<string>();
        private int currentPremadeMapIndex = 0;
        private const int requiredRows = 10;
        private const int requiredColumns = 15;

        public TurnManager turnManager { get; private set; }
        public List<Enemy> Enemies { get; private set; } = new List<Enemy>();
        private List<Enemy> enemiesToRemove = new List<Enemy>(); // New list to track dead enemies
        private bool playerCanMove = false; //  Track if player can move
        private TurnState lastLoggedState = TurnState.PlayerTurn;
        public HealthComponent PlayerHealth { get; private set; }

        private List<(float TimeRemaining, Action Callback)> delayedActions = new List<(float, Action)>();

        public Point PlayerGridPosition => playerGridPosition;

        public GameManager(GraphicsDeviceManager graphics, ContentManager content)
        {
            _graphics = graphics;
            _content = content;
            gameMap = new Map(requiredRows, requiredColumns);
            previousKeyboardState = Keyboard.GetState();
            turnManager = new TurnManager(this);

            PlayerHealth = new HealthComponent(5); // Set player health to 5
            PlayerHealth.OnHealthChanged += () => LogToFile($"Player took damage. Health: {PlayerHealth.CurrentHealth}");
            PlayerHealth.OnDeath += HandlePlayerDeath;
        }

        public void LoadContent()
        {
            playerTexture = _content.Load<Texture2D>("player");
            enemyTexture = _content.Load<Texture2D>("rat");
            openExitTexture = _content.Load<Texture2D>("OpenExitTile");
            damageFont = _content.Load<SpriteFont>("DamageFont");
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
                SpawnEnemies(2); 
            }

            turnManager.StartPlayerTurn();
        }

        public void ScheduleDelayedAction(float delay, Action action)
        {
            delayedActions.Add((delay, action));
        }

        public void QueueEnemyForRemoval(Enemy enemy)
        {
            if (!enemiesToRemove.Contains(enemy))
            {
                enemiesToRemove.Add(enemy);
            }
        }

        private void RemoveQueuedEnemies()
        {
            if (enemiesToRemove.Count > 0)
            {
                foreach (var enemy in enemiesToRemove)
                {
                    Enemies.Remove(enemy);
                    LogToFile($"Removed enemy at {enemy.GridPosition}.");
                }
                enemiesToRemove.Clear(); // Clear the queue after processing
                CheckExitTile(); // Ensure exit updates after enemy removal
            }
        }

        public void Update(GameTime gameTime)
        {
            RemoveQueuedEnemies();

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

            for (int i = damageTexts.Count - 1; i >= 0; i--)
            {
                damageTexts[i].Update(gameTime);
                if (damageTexts[i].IsExpired)
                {
                    damageTexts.RemoveAt(i);
                }
            }

            KeyboardState currentKeyboardState = Keyboard.GetState();

            // Ensure only the player moves during their turn
            if (playerCanMove && turnManager.IsPlayerTurn())
            {
                HandlePlayerTurn(currentKeyboardState);
            }

            // Ensure exit only works when all enemies are defeated
            if (gameMap.Tiles[playerGridPosition.Y, playerGridPosition.X] is ExitTile && Enemies.Count == 0)
            {
                LoadNextMap();
            }

            CheckExitTile();       

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

            foreach (var damageText in damageTexts)
            {
                damageText.Draw(spriteBatch);
            }

            spriteBatch.Draw(playerTexture, playerPosition, null, Color.White, 0f,
                new Vector2(playerTexture.Width / 2, playerTexture.Height / 2),
                Vector2.One, SpriteEffects.None, 0f);
            spriteBatch.End();

            // **Draw UI without the transformMatrix to keep it in the window coordinates**
            spriteBatch.Begin();

            // Draw the player health in the top left corner
            string healthText = $"Player Health: {PlayerHealth.CurrentHealth} / {PlayerHealth.MaxHealth}";
            Vector2 healthPosition = new Vector2(10, 10); // 10px from top and left corner
            spriteBatch.DrawString(damageFont, healthText, healthPosition, Color.Black);

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
            SpawnEnemies(2);
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
                Enemy enemyAtTarget = Enemies.FirstOrDefault(e => e.GridPosition == targetPosition && e.IsAlive);

                if (enemyAtTarget != null)
                {
                    int damage = 1;
                    enemyAtTarget.TakeDamage(damage);
                    LogToFile($"Player attacked enemy at {targetPosition}!");

                    Vector2 damagePosition = new Vector2(
                            enemyAtTarget.GridPosition.X * gameMap.TileWidth + gameMap.TileWidth / 2,
                            enemyAtTarget.GridPosition.Y * gameMap.TileHeight
                            );

                    AddDamageText($"-{damage}", damagePosition);

                    // If enemy died, remove it from list immediately
                    if (!enemyAtTarget.IsAlive)
                    {
                        Enemies.Remove(enemyAtTarget);
                        CheckExitTile(); // Ensure the exit updates
                    }

                    turnManager.EndPlayerTurn();
                }
                else if (IsCellAccessible(targetPosition.X, targetPosition.Y))
                {
                    // Move only if no enemy is present
                    playerGridPosition = targetPosition;
                    playerPosition = new Vector2(
                        playerGridPosition.X * gameMap.TileWidth + gameMap.TileWidth / 2,
                        playerGridPosition.Y * gameMap.TileHeight + gameMap.TileHeight / 2
                    );

                    turnManager.EndPlayerTurn(); // End turn after movement
                }
            }
        }

        public void AddDamageText(string text, Vector2 position)
        {
            damageTexts.Add(new DamageText(text, position, damageFont));
        }

        private void HandlePlayerDeath()
        {
            LogToFile("Player has died! Game Over.");
        }

        public void PlayerTakeDamage(int damage)
        {
            PlayerHealth.TakeDamage(damage);
            LogToFile($"Player took {damage} damage! Health: {PlayerHealth.CurrentHealth}");          

            if (!PlayerHealth.IsAlive)
            {
                LogToFile("Player has died. Game Over.");
                HandlePlayerDeath();
            }
        }

        public void SetPlayerCanMove(bool canMove)
        {
            playerCanMove = canMove;
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

        public void CheckExitTile()
        {
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
                        gameMap.Tiles[row, col] = new OpenExitTile(openExitTexture, tilePosition);
                    }
                }
            }
        }

        private void ResetPlayerPosition()
        {
            playerGridPosition = gameMap.SpawnPoint; // Use the correct spawn point
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
                    if (gameMap.IsTileWalkable(new Point(col, row)) && new Point(col, row) != gameMap.SpawnPoint)
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

                var enemy = new Enemy(enemyTexture, spawnPoint, gameMap, this);
                Enemies.Add(enemy);
                //LogToFile($"Spawned enemy #{i + 1} at {spawnPoint}");
            }
        }

        public void MarkEnemyForRemoval(Enemy enemy)
        {
            if (!enemiesToRemove.Contains(enemy))
            {
                enemiesToRemove.Add(enemy);
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
