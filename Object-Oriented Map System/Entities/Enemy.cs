using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Object_Oriented_Map_System.Managers;
using Object_Oriented_Map_System.MapSystem;
using Object_Oriented_Map_System.MapSystem.Tiles;
using System;

namespace Object_Oriented_Map_System.Entities
{
    public class Enemy
    {
        public Point GridPosition { get; private set; }
        private Vector2 worldPosition;
        private int health;
        private bool isStunned;
        private Texture2D texture;
        private Map gameMap;
        private GameManager gameManager;

        public bool IsStunned => isStunned;
        public bool IsAlive => health > 0;

        public Enemy(Texture2D enemyTexture, Point startGridPos, Map map, GameManager manager)
        {
            texture = enemyTexture;
            GridPosition = startGridPos;
            gameMap = map;
            gameManager = manager;
            worldPosition = new Vector2(GridPosition.X * gameMap.TileWidth, GridPosition.Y * gameMap.TileHeight);
            health = 3; // Default enemy health
        }

        public void TakeTurn()
        {
            if (!IsAlive) return;

            // Find a path to the player (basic pathfinding)
            Point targetPosition = gameManager.PlayerGridPosition;
            Point nextStep = FindPathToTarget(targetPosition);

            if (nextStep != GridPosition)
            {
                GridPosition = nextStep;
                worldPosition = new Vector2(GridPosition.X * gameMap.TileWidth, GridPosition.Y * gameMap.TileHeight);
            }
        }

        public void TakeDamage()
        {
            health--;
            isStunned = true;
        }

        public void RecoverFromStun()
        {
            isStunned = false;
        }

        private Point FindPathToTarget(Point target)
        {
            int dx = target.X - GridPosition.X;
            int dy = target.Y - GridPosition.Y;

            Point nextStep = GridPosition;

            if (Math.Abs(dx) > Math.Abs(dy)) // Move horizontally
            {
                nextStep = new Point(GridPosition.X + Math.Sign(dx), GridPosition.Y);
            }
            else // Move vertically
            {
                nextStep = new Point(GridPosition.X, GridPosition.Y + Math.Sign(dy));
            }

            // Check if the next step is walkable
            if (gameMap.IsTileWalkable(nextStep))
            {
                return nextStep;
            }

            return GridPosition; // If blocked, stay in place
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, worldPosition, Color.White);
        }
    }
}