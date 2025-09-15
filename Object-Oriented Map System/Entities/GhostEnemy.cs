using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Object_Oriented_Map_System.Managers;
using Object_Oriented_Map_System.MapSystem;
using Object_Oriented_Map_System.MapSystem.Tiles;
using System;
using System.IO;

namespace Object_Oriented_Map_System.Entities
{
    public class GhostEnemy : Enemy
    {
        public GhostEnemy(Texture2D texture, Point gridPosition, Map map, GameManager gameManager)
            : base(texture, gridPosition, map, gameManager)
        {
        }

        public override void TakeTurn(Action onComplete)
        {
            if (!IsAlive || TurnManager.Instance.IsPlayerTurn())
            {
                onComplete?.Invoke();
                return;
            }

            // Skip turn if necessary
            if (IsStunned)
            {
                IsStunned = false;
                onComplete?.Invoke();
                return;
            }

            // Find path to player using ghost's phasing ability
            Point targetPosition = FindPathToTarget(gameManager.player.PlayerGridPosition, ignoreWalls: true);

            if (targetPosition == gameManager.player.PlayerGridPosition)
            {
                int damage = 1;
                gameManager.player.PlayerHealth.TakeDamage(damage);

                Vector2 damagePosition = new Vector2(
                    gameManager.player.PlayerGridPosition.X * gameManager.gameMap.TileWidth + gameManager.gameMap.TileWidth / 2,
                    gameManager.player.PlayerGridPosition.Y * gameManager.gameMap.TileHeight
                );
                gameManager.AddDamageText($"-{damage}", damagePosition);
            }
            else
            {
                GridPosition = targetPosition;
                worldPosition = new Vector2(GridPosition.X * gameManager.gameMap.TileWidth, GridPosition.Y * gameManager.gameMap.TileHeight);
                LogToFile($"Ghost moved to {GridPosition}");
            }

            gameManager.ScheduleDelayedAction(0.3f, onComplete);
        }

        protected Point FindPathToTarget(Point target, bool ignoreWalls)
        {
            int dx = target.X - GridPosition.X;
            int dy = target.Y - GridPosition.Y;

            bool prioritizeHorizontal = Math.Abs(dx) > Math.Abs(dy);

            Point primaryMove = prioritizeHorizontal
                ? new Point(GridPosition.X + Math.Sign(dx), GridPosition.Y)
                : new Point(GridPosition.X, GridPosition.Y + Math.Sign(dy));

            Point secondaryMove = prioritizeHorizontal
                ? new Point(GridPosition.X, GridPosition.Y + Math.Sign(dy))
                : new Point(GridPosition.X + Math.Sign(dx), GridPosition.Y);

            // Ghosts ignore walls, so the move is valid without checking accessibility
            return primaryMove;
        }

        public override void TakeDamage(int damage)
        {
            if (!IsAlive) return;

            // Ghosts are immune to physical attacks unless it's a scroll
            if (!gameManager.LastAttackWasScroll)
            {
                LogToFile($"Ghost at {GridPosition} is immune to regular attacks.");
                return;
            }

            Health.TakeDamage(damage);

            Vector2 damageTextPosition = new Vector2(
                GridPosition.X * gameManager.gameMap.TileWidth,
                GridPosition.Y * gameManager.gameMap.TileHeight
            );

            gameManager.AddDamageText($"-{damage}", damageTextPosition);

            if (!IsAlive)
            {
                gameManager.MarkEnemyForRemoval(this);
                LogToFile($"Ghost at {GridPosition} defeated by Scroll!");
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
