using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Object_Oriented_Map_System.Managers;
using Object_Oriented_Map_System.MapSystem;
using Object_Oriented_Map_System.MapSystem.Tiles;
using System;
using System.IO;

namespace Object_Oriented_Map_System.Entities
{
    public class Enemy
    {
        public Point GridPosition { get; private set; }
        private Vector2 worldPosition;
        private Texture2D texture;
        private Map gameMap;
        private GameManager gameManager;

        public HealthComponent Health { get; private set; } // Enemy Health System
        public bool IsAlive => Health.IsAlive; // Check if enemy is alive
        public bool IsStunned { get; private set; } = false; // Enemy Stun Mechanic
        private bool isFlipped = false;

        public Enemy(Texture2D enemyTexture, Point startGridPos, Map map, GameManager manager)
        {
            texture = enemyTexture;
            GridPosition = startGridPos;
            gameMap = map;
            gameManager = manager;
            worldPosition = new Vector2(GridPosition.X * gameMap.TileWidth, GridPosition.Y * gameMap.TileHeight);

            Health = new HealthComponent(2); // Enemy starts with 2 health
            Health.OnHealthChanged += () => LogToFile($"Enemy at {GridPosition} took damage. Health: {Health.CurrentHealth}");
            Health.OnDeath += Die;
        }

        public void TakeTurn(Action onComplete)
        {
            if (!gameManager.turnManager.IsEnemyTurn())
            {
               // LogToFile($"ERROR: Enemy at {GridPosition} tried to move outside EnemyTurn!");
                return;
            }

            if (!IsAlive)
            {
                LogToFile($"Enemy at {GridPosition} is dead and cannot act.");
                onComplete?.Invoke();
                return;
            }

            if (IsStunned)
            {
                LogToFile($"Enemy at {GridPosition} is stunned and skips turn.");
                IsStunned = false;
                onComplete?.Invoke();
                return;
            }
            isFlipped = false;

            LogToFile($"ENEMY TURN: Enemy at {GridPosition} is preparing to move...");

            Point targetPosition = FindPathToTarget(gameManager.PlayerGridPosition);

            if (targetPosition == gameManager.PlayerGridPosition)
            {
                gameManager.PlayerHealth.TakeDamage(1);
                LogToFile($"Enemy at {GridPosition} attacked the player! Player Health: {gameManager.PlayerHealth.CurrentHealth}");

                gameManager.ScheduleDelayedAction(0.3f, onComplete);
                return;
            }

            if (targetPosition != GridPosition)
            {
                GridPosition = targetPosition;
                worldPosition = new Vector2(GridPosition.X * gameMap.TileWidth, GridPosition.Y * gameMap.TileHeight);
                LogToFile($"Enemy moved to {GridPosition}");
            }
            else
            {
                LogToFile($"Enemy at {GridPosition} found no valid move.");
            }

            gameManager.ScheduleDelayedAction(0.3f, onComplete);
        }

        public void TakeDamage(int damage)
        {
            if (!IsAlive) return;

            Health.TakeDamage(damage);
            LogToFile($"Enemy at {GridPosition} took {damage} damage! Health: {Health.CurrentHealth}");

            Vector2 damageTextPosition = new Vector2(GridPosition.X * gameMap.TileWidth, GridPosition.Y * gameMap.TileHeight);

            // Add floating damage number
            gameManager.AddDamageText(damage.ToString(), damageTextPosition);

            if (Health.IsAlive)
            {
                IsStunned = true; // Enemy gets stunned for next turn
                isFlipped = true; //flip enemy upside down when stunned
                LogToFile($"Enemy at {GridPosition} is stunned and will skip its next turn!");
            }
        }

        public void Die()
        {
            if (!IsAlive) return;

            LogToFile($"Enemy at {GridPosition} has died.");

            // Remove from the game logic immediately
            gameManager.Enemies.Remove(this);

            // Check if the exit should open after enemy removal
            gameManager.CheckExitTile();
        }


        private Point FindPathToTarget(Point target)
        {
            int dx = target.X - GridPosition.X;
            int dy = target.Y - GridPosition.Y;

            Point nextStep = GridPosition;

            if (Math.Abs(dx) > Math.Abs(dy))
            {
                nextStep = new Point(GridPosition.X + Math.Sign(dx), GridPosition.Y);
            }
            else
            {
                nextStep = new Point(GridPosition.X, GridPosition.Y + Math.Sign(dy));
            }

            return gameMap.IsTileWalkable(nextStep) ? nextStep : GridPosition;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            SpriteEffects spriteEffects = isFlipped ? SpriteEffects.FlipVertically : SpriteEffects.None;

            spriteBatch.Draw(texture, worldPosition, null, Color.White, 0f, Vector2.Zero, 1f, spriteEffects, 0f);
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