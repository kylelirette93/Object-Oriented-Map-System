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
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;


namespace Object_Oriented_Map_System.Managers
{
    public enum GameState
    {
        Normal,
        FireballAiming,
        BombAiming,
    }

    public class GameManager
    {
        private ContentManager _content;
        private GraphicsDeviceManager _graphics;
        public Map gameMap;
        private Texture2D enemyTexture;
        private Texture2D rangedEnemyTexture;
        private Texture2D ghostEnemyTexture;
        private Texture2D openExitTexture;
        public Texture2D fireballTexture;
        private Texture2D lightningScrollTexture;
        public int CurrentStage { get; private set; } = 1;

        private bool isFadingOut = false;
        private float fadeTimer = 0f;
        private float fadeDuration = 3f;
        private float playerAlpha = 1f;
        public bool IsFadeComplete => isFadingOut && fadeTimer >= fadeDuration;

        // This will be set by Game1 when the fade is done
        public Action OnFadeComplete;

        private SpriteFont damageFont;
        private List<DamageText> damageTexts = new List<DamageText>();

        private List<string> premadeMapFiles = new List<string>();
        private int currentPremadeMapIndex = 0;
        private const int requiredRows = 10;
        private const int requiredColumns = 15;

        public List<Enemy> Enemies { get; private set; } = new List<Enemy>();
        private List<Enemy> enemiesToRemove = new List<Enemy>(); // New list to track dead enemies
        private TurnState lastLoggedState = TurnState.PlayerTurn;

        public static Texture2D whiteTexture;

        public List<Item> Items { get; private set; } = new List<Item>();
        private Texture2D healthPotionTexture;

        private GameState gameState = GameState.Normal;
        private FireballScroll activeFireball;

        public List<Fireball> ActiveFireballs { get; private set; } = new List<Fireball>();

        public Texture2D bombTexture;
        public List<BombProjectile> ActiveBombs { get; private set; } = new List<BombProjectile>();
        private BombItem activeBomb;

        public List<ExplosionEffect> ActiveExplosions { get; private set; } = new List<ExplosionEffect>();

        InputManager input;

        public bool LastAttackWasScroll {  get; set; }

        public Player player;

        // Kyle - Made the game manager accessible from anywhere.
        public static GameManager Instance;

        public List<Shop> Shops = new List<Shop>();
        Shop activeShop = null;
        EventBus events = new EventBus();
      
        public GameManager(GraphicsDeviceManager graphics, ContentManager content)
        {
            Instance = this;
            _graphics = graphics;
            _content = content;
            gameMap = new Map(requiredRows, requiredColumns);
            player = new Player(content);
            input = new InputManager(player);
            player.PlayerHealth.OnHealthChanged += () => LogToFile($"Player took damage. Health: {player.PlayerHealth.CurrentHealth}");
            player.PlayerHealth.OnDeath += HandlePlayerDeath;
        }

        public void LoadContent()
        {
            enemyTexture = _content.Load<Texture2D>("rat");
            rangedEnemyTexture = _content.Load<Texture2D>("EvilWizard");
            ghostEnemyTexture = _content.Load<Texture2D>("Ghost");
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
                Shops.Clear();
                gameMap.PlacedShops.Clear();
                gameMap.GenerateRandomMap();
            }

            ResetPlayerPosition();

            //  Ensure map is fully loaded before spawning enemies
            if (gameMap.Rows > 0 && gameMap.Columns > 0)
            {
                SpawnEnemies(2);
                SpawnItems(3);
            }

            TurnManager.Instance.StartPlayerTurn();
        }

        public void ScheduleDelayedAction(float delay, Action action)
        {
            input.delayedActions.Add((delay, action));
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
                    // Reward player with currency on enemy death
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
                TurnManager.Instance.EndPlayerTurn();
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
                activeBomb.ThrowBomb(direction);
                ExitBombAimingMode();
                TurnManager.Instance.EndPlayerTurn();
            }
        }

        public void TriggerExplosionEffect(List<Point> explosionTiles)
        {
            ExplosionEffect explosionEffect = new ExplosionEffect(explosionTiles);
            ActiveExplosions.Add(explosionEffect);
        }

        public void Update(GameTime gameTime)
        {
            RemoveQueuedEnemies();

            // Process scheduled actions with delays
            for (int i = input.delayedActions.Count - 1; i >= 0; i--)
            {
                var (timeRemaining, callback) = input.delayedActions[i];
                timeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (timeRemaining <= 0)
                {
                    callback.Invoke();
                    input.delayedActions.RemoveAt(i);
                }
                else
                {
                    input.delayedActions[i] = (timeRemaining, callback); // Update remaining time
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
            
            // Check if a shop is active, if so update it for input.
            if (activeShop != null)
            {
                activeShop.Update(gameTime);
                if (activeShop.IsVisited == false)
                {
                    // Close the shop.
                    activeShop = null; 
                }
            }
            else
            {
                HandleItemUsage(currentKeyboardState);
            }

            // Prioritize aiming modes
            if (gameState == GameState.FireballAiming)
            {
                HandleFireballAiming(currentKeyboardState);
            }
            else if (gameState == GameState.BombAiming)
            {
                HandleBombAiming(currentKeyboardState);
            }
            else if (player.PlayerCanMove && TurnManager.Instance.IsPlayerTurn())
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
            if (gameMap.Tiles[player.PlayerGridPosition.Y, player.PlayerGridPosition.X] is ExitTile && Enemies.Count == 0)
            {
                LoadNextMap();
            }


            CheckExitTile();

            if (isFadingOut)
            {
                fadeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                playerAlpha = MathHelper.Lerp(1f, 0f, fadeTimer / fadeDuration);

                if (fadeTimer >= fadeDuration)
                {
                    playerAlpha = 0f;
                    isFadingOut = false;

                    OnFadeComplete?.Invoke(); // Notify Game1
                }
            }

            // Kyle - Set state in input manager.
            input.SetState(currentKeyboardState);

        }

        // Kyle -- Commented out because it wasn't ever called to begijn with.
        /*public void AllowPlayerInput(bool canMove)
        {
            playerCanMove = canMove;
        }*/

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


            player.Draw(spriteBatch);
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
            string healthText = $"Player Health: {player.PlayerHealth.CurrentHealth} / {player.PlayerHealth.MaxHealth}";
            Vector2 healthPosition = new Vector2(10, 10); // 10px from top and left corner
            spriteBatch.DrawString(damageFont, healthText, healthPosition, Color.Black);

            // Draw the Inventory
            Vector2 inventoryPosition = new Vector2(10, 50); // Slightly below health bar
            player.PlayerInventory.Draw(spriteBatch, damageFont, inventoryPosition, whiteTexture);

            // Stage Numbers
            string stageText = $"Stage: {CurrentStage}";
            Vector2 stageSize = damageFont.MeasureString(stageText);
            Vector2 stagePosition = new Vector2(
                _graphics.PreferredBackBufferWidth - stageSize.X - 10, // 10px padding from right
                10 // top padding
            );

            foreach (var shop in gameMap.PlacedShops)
            {
                if (shop.IsVisited)
                shop.Draw(spriteBatch);
            }
            spriteBatch.DrawString(damageFont, stageText, stagePosition, Color.Black);

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
            CurrentStage++;

            if (currentPremadeMapIndex < premadeMapFiles.Count)
            {
                LoadPremadeMap(premadeMapFiles[currentPremadeMapIndex]);
                currentPremadeMapIndex++;
            }
            else
            {
                Shops.Clear();
                gameMap.PlacedShops.Clear();
                gameMap.GenerateRandomMap();
            }

            ResetPlayerPosition();
            Enemies.Clear();
            Items.Clear();
            SpawnEnemies(2);
            SpawnItems(3);
        }

        public void HandlePlayerTurn(KeyboardState currentKeyboardState)
        {

            if (!TurnManager.Instance.IsPlayerTurn()) return; // Prevents movement during enemy turns

            Point targetPosition = player.PlayerGridPosition;

            if (activeShop != null)
            {
                if (!targetPosition.Equals(activeShop.GridPosition))
                {
                    LogToFile("Player left the shop.");
                    activeShop.Leave();
                    activeShop = null;
                }
            }

            // Kyle - Get the keyboard state from the input manager.
            input.GetState(out currentKeyboardState);

            if (currentKeyboardState.IsKeyDown(Keys.Up) && !input.PreviousKeyboardState.IsKeyDown(Keys.Up))
                targetPosition.Y -= 1;
            if (currentKeyboardState.IsKeyDown(Keys.Down) && !input.PreviousKeyboardState.IsKeyDown(Keys.Down))
                targetPosition.Y += 1;
            if (currentKeyboardState.IsKeyDown(Keys.Left) && !input.PreviousKeyboardState.IsKeyDown(Keys.Left))
                targetPosition.X -= 1;
            if (currentKeyboardState.IsKeyDown(Keys.Right) && !input.PreviousKeyboardState.IsKeyDown(Keys.Right))
                targetPosition.X += 1;

            if (targetPosition != player.PlayerGridPosition)
            {
                Enemy enemyAtTarget = Enemies.FirstOrDefault(e => e.GridPosition == targetPosition && e.IsAlive);

                Shop shopAtTarget = gameMap.PlacedShops
                .FirstOrDefault(shop => shop.GridPosition.Equals(targetPosition));
                if (shopAtTarget != null)
                {
                    shopAtTarget.LoadContent(_content);
                    shopAtTarget.Visit(player);
                    activeShop = shopAtTarget;
                    LogToFile("Player entered a shop.");
                }


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
                        EventBus.Instance.Publish(EventType.EarnCash, 10);
                        Enemies.Remove(enemyAtTarget);
                        CheckExitTile(); // Ensure the exit updates
                    }

                }
                else if (IsCellAccessible(targetPosition.X, targetPosition.Y))
                {
                    // Move only if no enemy is present
                    player.PlayerGridPosition = targetPosition;
                    player.PlayerPosition = new Vector2(
                        player.PlayerGridPosition.X * gameMap.TileWidth + gameMap.TileWidth / 2,
                        player.PlayerGridPosition.Y * gameMap.TileHeight + gameMap.TileHeight / 2
                    );

                    TurnManager.Instance.EndPlayerTurn(); // End turn after movement
                }
            }
            Item itemAtTarget = Items.FirstOrDefault(item => item.GridPosition == targetPosition && !item.IsPickedUp);

            if (itemAtTarget != null)
            {
                itemAtTarget.OnPickup();
                LogToFile($"Player picked up item at {targetPosition}");
            }
        }

        private void HandleItemUsage(KeyboardState currentKeyboardState)
        {
            for (int i = 0; i < 5; i++)
            {
                Keys key = Keys.D1 + i; // Keys 1-5
                if (currentKeyboardState.IsKeyDown(key) && !input.PreviousKeyboardState.IsKeyDown(key))
                {
                    // Ensure the index is within the inventory size
                    if (player.PlayerInventory.Items.Count > i)
                    {
                        // Use the item if it's present in the inventory
                        LogToFile($"Using item at slot {i + 1}");
                        player.PlayerInventory.UseItem(i);
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
            player.OnPlayerDeath?.Invoke();
            isFadingOut = true;
            fadeTimer = 0f;
            playerAlpha = 1f;
        }

        public void PlayerTakeDamage(int damage)
        {
            player.PlayerHealth.TakeDamage(damage);
            //LogToFile($"Player took {damage} damage! Health: {PlayerHealth.CurrentHealth}");          

            if (!player.PlayerHealth.IsAlive)
            {
                LogToFile("Player has died. Game Over.");
                HandlePlayerDeath();
            }
        }

        public void SetPlayerCanMove(bool canMove)
        {
            player.PlayerCanMove = canMove;
        }

        public bool IsCellAccessible(int col, int row)
        {
            if (row < 0 || row >= gameMap.Rows || col < 0 || col >= gameMap.Columns)
                return false;

            Tile destinationTile = gameMap.Tiles[row, col];

            if (destinationTile is OpenExitTile) return true;
            if (destinationTile is ShopTile) return true;
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
            player.PlayerGridPosition = gameMap.SpawnPoint; // Use the correct spawn point
            player.PlayerPosition = new Vector2(
                player.PlayerGridPosition.X * gameMap.TileWidth + gameMap.TileWidth / 2,
                player.PlayerGridPosition.Y * gameMap.TileHeight + gameMap.TileHeight / 2);
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

                // Randomly choose enemy type
                int enemyType = rand.Next(3); // 0 = Regular, 1 = Ranged, 2 = Ghost

                if (enemyType == 0)
                {
                    var enemy = new Enemy(enemyTexture, spawnPoint, gameMap, this);
                    Enemies.Add(enemy);
                    LogToFile($"Spawned Enemy at {spawnPoint}");
                }
                else if (enemyType == 1)
                {
                    var rangedEnemy = new RangedEnemy(rangedEnemyTexture, spawnPoint, gameMap, this);
                    Enemies.Add(rangedEnemy);
                    LogToFile($"Spawned RangedEnemy at {spawnPoint}");
                }
                else
                {
                    var ghostEnemy = new GhostEnemy(ghostEnemyTexture, spawnPoint, gameMap, this);
                    Enemies.Add(ghostEnemy);
                    LogToFile($"Spawned GhostEnemy at {spawnPoint}");
                }
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

        public void LogToFile(string message)
        {
            string logPath = "debug_log.txt";
            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
    }
}
