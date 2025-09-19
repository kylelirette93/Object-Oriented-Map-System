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
        public Point GridPosition { get; protected set; }
        protected Vector2 worldPosition;
        private Texture2D texture;
        private Map gameMap;
        protected GameManager gameManager;

        public HealthComponent Health { get; private set; } // Enemy Health System
        public bool IsAlive => Health.IsAlive; // Check if enemy is alive
        public bool IsStunned { get; protected set; } = false; // Enemy Stun Mechanic
        protected bool isFlipped = false;

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

        public virtual void TakeTurn(Action onComplete)
        {
            if (!TurnManager.Instance.IsEnemyTurn() && TurnManager.Instance.IsPlayerTurn())
            {
                // LogToFile($"ERROR: Enemy at {GridPosition} tried to move outside EnemyTurn!");
                onComplete?.Invoke();
                return;
            }

            if (!IsAlive)
            {
                //LogToFile($"Enemy at {GridPosition} is dead and cannot act.");
                onComplete?.Invoke();
                return;
            }

            if (IsStunned)
            {
                //LogToFile($"Enemy at {GridPosition} is stunned and skips turn.");
                IsStunned = false;
                onComplete?.Invoke();
                return;
            }
            isFlipped = false;

            //LogToFile($"ENEMY TURN: Enemy at {GridPosition} is preparing to move...");

            Point targetPosition = FindPathToTarget(gameManager.player.PlayerGridPosition);

            if (targetPosition == gameManager.player.PlayerGridPosition)
            {
                int damage = 1;
                gameManager.player.PlayerHealth.TakeDamage(damage);

                Vector2 damagePosition = new Vector2(
                    gameManager.player.PlayerGridPosition.X * gameMap.TileWidth + gameMap.TileWidth / 2,
                    gameManager.player.PlayerGridPosition.Y * gameMap.TileHeight - 10  // Slight offset above the player
                );
                gameManager.AddDamageText($"-{damage}", damagePosition);

                gameManager.ScheduleDelayedAction(0.3f, onComplete);
                return;
            }

            if (targetPosition != GridPosition)
            {
                GridPosition = targetPosition;
                worldPosition = new Vector2(GridPosition.X * gameMap.TileWidth, GridPosition.Y * gameMap.TileHeight);
            }

            gameManager.ScheduleDelayedAction(0.3f, onComplete);
        }

        public virtual void TakeDamage(int damage)
        {
            if (!IsAlive) return;

            Health.TakeDamage(damage);

            Vector2 damageTextPosition = new Vector2(GridPosition.X * gameMap.TileWidth, GridPosition.Y * gameMap.TileHeight);

            if (Health.IsAlive)
            {
                IsStunned = true; // Enemy gets stunned for next turn
                isFlipped = true; //flip enemy upside down when stunned              
            }
        }

        public virtual void Die()
        {
            if (!IsAlive) return;

            Console.WriteLine("Enemy has died.");

            // Remove from the game logic immediately
            gameManager.MarkEnemyForRemoval(this);

            // Check if the exit should open after enemy removal
            gameManager.CheckExitTile();
        }


        protected virtual Point FindPathToTarget(Point target)
        {
            int dx = target.X - GridPosition.X;
            int dy = target.Y - GridPosition.Y;

            // Determine whether to move horizontally or vertically first
            bool prioritizeHorizontal = Math.Abs(dx) > Math.Abs(dy);

            Point primaryMove = prioritizeHorizontal
                ? new Point(GridPosition.X + Math.Sign(dx), GridPosition.Y)  // Prioritize horizontal
                : new Point(GridPosition.X, GridPosition.Y + Math.Sign(dy)); // Prioritize vertical

            Point secondaryMove = prioritizeHorizontal
                ? new Point(GridPosition.X, GridPosition.Y + Math.Sign(dy))  // Secondary is vertical
                : new Point(GridPosition.X + Math.Sign(dx), GridPosition.Y); // Secondary is horizontal

            // Try the best option first
            if (gameMap.IsTileWalkable(primaryMove)) return primaryMove;
            if (gameMap.IsTileWalkable(secondaryMove)) return secondaryMove;

            // If all else fails, try moving in the opposite directions
            Point oppositeHorizontal = new Point(GridPosition.X - Math.Sign(dx), GridPosition.Y);
            Point oppositeVertical = new Point(GridPosition.X, GridPosition.Y - Math.Sign(dy));

            if (gameMap.IsTileWalkable(oppositeHorizontal)) return oppositeHorizontal;
            if (gameMap.IsTileWalkable(oppositeVertical)) return oppositeVertical;

            return GridPosition; // Stay in place if no valid move found
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsAlive) return;

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