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
    public enum GameState
    {
        Normal,
        FireballAiming,
        BombAiming
    }

    public class GameManager
    {
        private ContentManager _content;
        private GraphicsDeviceManager _graphics;
        public Map gameMap;
        private Texture2D playerTexture;
        private Texture2D enemyTexture;
        private Texture2D openExitTexture;
        public Texture2D fireballTexture;
        private Texture2D lightningScrollTexture;
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

        public Inventory PlayerInventory { get; private set; }
        private Texture2D whiteTexture;

        public List<Item> Items { get; private set; } = new List<Item>();
        private Texture2D healthPotionTexture;

        private GameState gameState = GameState.Normal;
        private FireballScroll activeFireball;

        public List<Fireball> ActiveFireballs { get; private set; } = new List<Fireball>();

        public Texture2D bombTexture;
        public List<BombProjectile> ActiveBombs { get; private set; } = new List<BombProjectile>();
        private BombItem activeBomb;

        public List<ExplosionEffect> ActiveExplosions { get; private set; } = new List<ExplosionEffect>();

        public GameManager(GraphicsDeviceManager graphics, ContentManager content)
        {
            _graphics = graphics;
            _content = content;
            gameMap = new Map(requiredRows, requiredColumns);
            previousKeyboardState = Keyboard.GetState();
            turnManager = new TurnManager(this);
            PlayerInventory = new Inventory();

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
            whiteTexture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
            whiteTexture.SetData(new Color[] { Color.White });
            healthPotionTexture = _content.Load<Texture2D>("HealthPotion");
            fireballTexture = _content.Load<Texture2D>("FireballScroll");
            lightningScrollTexture = _content.Load<Texture2D>("LightningScroll");
            bombTexture = _content.Load<Texture2D>("Bomb");

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
                SpawnItems(3);
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

        public void EnterFireballAimingMode(FireballScroll fireball)
        {
            if (fireball == null) return; // Safety Check
            gameState = GameState.FireballAiming;
            activeFireball = fireball;
            LogToFile("Player is aiming a Fireball. Choose a direction.");
        }

        private void ExitFireballAimingMode()
        {
            gameState = GameState.Normal;
            activeFireball = null;
        }

        private void HandleFireballAiming(KeyboardState currentKeyboardState)
        {
            Point direction = Point.Zero;

            if (currentKeyboardState.IsKeyDown(Keys.Up)) direction = new Point(0, -1);
            if (currentKeyboardState.IsKeyDown(Keys.Down)) direction = new Point(0, 1);
            if (currentKeyboardState.IsKeyDown(Keys.Left)) direction = new Point(-1, 0);
            if (currentKeyboardState.IsKeyDown(Keys.Right)) direction = new Point(1, 0);

            if (direction != Point.Zero)
            {
                activeFireball.CastFireball(this, direction);
                ExitFireballAimingMode();
                turnManager.EndPlayerTurn();
            }
        }

        public void EnterBombAimingMode(BombItem bomb)
        {
            if (bomb == null) return;
            gameState = GameState.BombAiming;
            activeBomb = bomb;
            LogToFile("Player is aiming a Bomb. Choose a direction.");
        }

        private void ExitBombAimingMode()
        {
            gameState = GameState.Normal;
            activeBomb = null;
        }

        private void HandleBombAiming(KeyboardState currentKeyboardState)
        {
            Point direction = Point.Zero;

            if (currentKeyboardState.IsKeyDown(Keys.Up)) direction = new Point(0, -1);
            if (currentKeyboardState.IsKeyDown(Keys.Down)) direction = new Point(0, 1);
            if (currentKeyboardState.IsKeyDown(Keys.Left)) direction = new Point(-1, 0);
            if (currentKeyboardState.IsKeyDown(Keys.Right)) direction = new Point(1, 0);

            if (direction != Point.Zero && activeBomb != null)
            {
                activeBomb.ThrowBomb(this, direction);
                ExitBombAimingMode();
                turnManager.EndPlayerTurn();
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
            HandleItemUsage(currentKeyboardState);

            // Prioritize aiming modes
            if (gameState == GameState.FireballAiming)
            {
                HandleFireballAiming(currentKeyboardState);
            }
            else if (gameState == GameState.BombAiming)
            {
                HandleBombAiming(currentKeyboardState);
            }
            else if (playerCanMove && turnManager.IsPlayerTurn())
            {
                HandlePlayerTurn(currentKeyboardState);
            }

            for (int i = ActiveFireballs.Count - 1; i >= 0; i--)
            {
                ActiveFireballs[i].Update();
                if (!ActiveFireballs[i].IsActive)
                {
                    ActiveFireballs.RemoveAt(i);
                }
            }

            for (int i = ActiveBombs.Count - 1; i >= 0; i--)
            {
                ActiveBombs[i].Update();
                if (!ActiveBombs[i].IsActive)
                {
                    ActiveBombs.RemoveAt(i);
                }
            }

            // Update explosions
            for (int i = ActiveExplosions.Count - 1; i >= 0; i--)
            {
                ActiveExplosions[i].Update(gameTime);
                if (!ActiveExplosions[i].IsActive)
                {
                    ActiveExplosions.RemoveAt(i);
                }
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
                if (enemy.IsAlive) // Ensure only alive enemies are drawn
                {
                    enemy.Draw(spriteBatch);
                }
            }

            foreach (var item in Items)
            {
                item.Draw(spriteBatch, gameMap.TileWidth, gameMap.TileHeight);
            }

            foreach (var fireball in ActiveFireballs)
            {
                fireball.Draw(spriteBatch, gameMap.TileWidth, gameMap.TileHeight);
            }

            foreach (var bomb in ActiveBombs)
            {
                bomb.Draw(spriteBatch, gameMap.TileWidth, gameMap.TileHeight);
            }

            // Draw explosions on top of tiles
            foreach (var explosion in ActiveExplosions)
            {
                explosion.Draw(spriteBatch, whiteTexture, gameMap.TileWidth, gameMap.TileHeight);
            }

            foreach (var damageText in damageTexts)
            {
                damageText.Draw(spriteBatch);
            }

            spriteBatch.Draw(playerTexture, playerPosition, null, Color.White, 0f,
                new Vector2(playerTexture.Width / 2, playerTexture.Height / 2),
                Vector2.One, SpriteEffects.None, 0f);
            spriteBatch.End();

            if (gameState == GameState.FireballAiming)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(damageFont, "Choose Fireball Direction (Arrow Keys)", new Vector2(10, 90), Color.Red);
                spriteBatch.End();
            }
            else if (gameState == GameState.BombAiming)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(damageFont, "Choose Bomb Direction (Arrow Keys)", new Vector2(10, 90), Color.Orange);
                spriteBatch.End();
            }

            // **Draw UI without the transformMatrix to keep it in the window coordinates**
            spriteBatch.Begin();

            // Draw the player health in the top left corner
            string healthText = $"Player Health: {PlayerHealth.CurrentHealth} / {PlayerHealth.MaxHealth}";
            Vector2 healthPosition = new Vector2(10, 10); // 10px from top and left corner
            spriteBatch.DrawString(damageFont, healthText, healthPosition, Color.Black);

            // Draw the Inventory
            Vector2 inventoryPosition = new Vector2(10, 50); // Slightly below health bar
            PlayerInventory.Draw(spriteBatch, damageFont, inventoryPosition, whiteTexture);

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
            SpawnItems(3);
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

            Item itemAtTarget = Items.FirstOrDefault(item => item.GridPosition == targetPosition && !item.IsPickedUp);

            if (itemAtTarget != null)
            {
                itemAtTarget.OnPickup(this);
                LogToFile($"Player picked up item at {targetPosition}");
            }
        }

        private void HandleItemUsage(KeyboardState currentKeyboardState)
        {
            for (int i = 0; i < 5; i++)
            {
                Keys key = Keys.D1 + i; // Keys 1-5
                if (currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key))
                {
                    // Ensure the index is within the inventory size
                    if (PlayerInventory.Items.Count > i)
                    {
                        // Use the item if it's present in the inventory
                        LogToFile($"Using item at slot {i + 1}");
                        PlayerInventory.UseItem(i, this);
                    }
                    else
                    {
                        LogToFile($"No item in slot {i + 1}.");
                    }
                }
            }
        }

        public void AddDamageText(string text, Vector2 position)
        {
            damageTexts.Add(new DamageText(text, position, damageFont));
        }

        private void HandlePlayerDeath()
        {
            //LogToFile("Player has died! Game Over.");
        }

        public void PlayerTakeDamage(int damage)
        {
            PlayerHealth.TakeDamage(damage);
            //LogToFile($"Player took {damage} damage! Health: {PlayerHealth.CurrentHealth}");          

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

        public bool IsCellAccessible(int col, int row)
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

        private void SpawnItems(int count)
        {
            Random rand = new Random();
            List<Point> availableTiles = new List<Point>();

            // Ensure it checks within bounds and only on walkable tiles
            for (int row = 1; row < gameMap.Rows - 1; row++) // Avoid the border walls
            {
                for (int col = 1; col < gameMap.Columns - 1; col++)
                {
                    Point point = new Point(col, row);
                    if (gameMap.IsTileWalkable(point) && point != gameMap.SpawnPoint)
                    {
                        availableTiles.Add(point);
                    }
                }
            }

            LogToFile($"Available tiles for items: {availableTiles.Count}");

            if (availableTiles.Count == 0)
            {
                LogToFile("No valid tiles available to spawn items.");
                return;
            }

            for (int i = 0; i < count && availableTiles.Count > 0; i++)
            {
                int index = rand.Next(availableTiles.Count);
                Point spawnPoint = availableTiles[index];
                availableTiles.RemoveAt(index);

                int itemType = rand.Next(4); // 0 for HealthPotion, 1 for FireballScroll, 2 for LightningScroll, 3 for Bomb

                if (itemType == 0)
                {
                    var healthPotion = new HealthPotion(healthPotionTexture, spawnPoint);
                    Items.Add(healthPotion);
                    LogToFile($"Spawned HealthPotion at {spawnPoint}");
                }
                else if (itemType == 1)
                {
                    var fireballScroll = new FireballScroll(fireballTexture, spawnPoint);
                    Items.Add(fireballScroll);
                    LogToFile($"Spawned FireballScroll at {spawnPoint}");
                }
                else if (itemType == 2)
                {
                    var lightningScroll = new LightningScroll(lightningScrollTexture, spawnPoint);
                    Items.Add(lightningScroll);
                    LogToFile($"Spawned LightningScroll at {spawnPoint}");
                }
                else
                {
                    var bomb = new BombItem(bombTexture, spawnPoint);
                    Items.Add(bomb);
                    LogToFile($"Spawned BombItem at {spawnPoint}");
                }
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
