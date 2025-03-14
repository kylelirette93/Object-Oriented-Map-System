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

        public bool IsAlive { get; private set; } = true;

        public Enemy(Texture2D enemyTexture, Point startGridPos, Map map, GameManager manager)
        {
            texture = enemyTexture;
            GridPosition = startGridPos;
            gameMap = map;
            gameManager = manager;
            worldPosition = new Vector2(GridPosition.X * gameMap.TileWidth, GridPosition.Y * gameMap.TileHeight);
        }

        public void TakeTurn(Action onComplete)
        {
            if (!IsAlive)
            {
                LogToFile($"Enemy at {GridPosition} is dead and cannot act.");
                onComplete?.Invoke(); // Ensures the turn system continues even if the enemy is dead
                return;
            }

            LogToFile($"ENEMY TURN: Enemy at {GridPosition} is preparing to move...");

            Point targetPosition = FindPathToTarget(gameManager.PlayerGridPosition);

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

            // Delay ensures enemies move one by one instead of all at once
            gameManager.ScheduleDelayedAction(0.3f, onComplete);
        }

        private Point FindPathToTarget(Point target)
        {
            int dx = target.X - GridPosition.X;
            int dy = target.Y - GridPosition.Y;

            Point nextStep = GridPosition;

            // Prioritize horizontal movement
            if (Math.Abs(dx) > Math.Abs(dy))
            {
                nextStep = new Point(GridPosition.X + Math.Sign(dx), GridPosition.Y);
            }
            else
            {
                nextStep = new Point(GridPosition.X, GridPosition.Y + Math.Sign(dy));
            }

            //  Ensure tile is walkable
            if (gameMap.IsTileWalkable(nextStep))
            {
                return nextStep;
            }

            //  If preferred move is blocked, try the other direction
            if (Math.Abs(dx) > Math.Abs(dy))
            {
                nextStep = new Point(GridPosition.X, GridPosition.Y + Math.Sign(dy));
            }
            else
            {
                nextStep = new Point(GridPosition.X + Math.Sign(dx), GridPosition.Y);
            }

            return gameMap.IsTileWalkable(nextStep) ? nextStep : GridPosition;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, worldPosition, Color.White);
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