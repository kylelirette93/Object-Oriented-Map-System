using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Object_Oriented_Map_System.Managers;
using Object_Oriented_Map_System.MapSystem;
using Object_Oriented_Map_System.MapSystem.Tiles;

namespace Object_Oriented_Map_System.Entities
{
    public class FireballScroll : Item
    {
        public int Damage { get; private set; } = 3;

        public FireballScroll(Texture2D texture, Point gridPosition)
            : base(texture, gridPosition)
        {
        }

        public override void OnPickup(GameManager gameManager)
        {
            base.OnPickup(gameManager);
            gameManager.PlayerInventory.AddItem(this);
        }

        public override void Use(GameManager gameManager)
        {
            if (gameManager.PlayerHealth.IsAlive)
            {
                // Trigger the aiming mode for Fireball
                gameManager.EnterFireballAimingMode(this);
                LogToFile("Player used Fireball Scroll. Choose a direction to cast it.");
            }
        }

        public override void Draw(SpriteBatch spriteBatch, int tileWidth, int tileHeight)
        {
            if (!IsPickedUp && Texture != null)
            {
                Vector2 worldPosition = new Vector2(GridPosition.X * tileWidth, GridPosition.Y * tileHeight);
                spriteBatch.Draw(Texture, worldPosition, Color.White);
            }
        }

        public void CastFireball(GameManager gameManager, Point direction)
        {
            Point currentPosition = gameManager.PlayerGridPosition;

            while (true)
            {
                currentPosition += direction;

                // Check if the current position is within bounds
                if (currentPosition.X < 0 || currentPosition.Y < 0 ||
                    currentPosition.X >= gameManager.gameMap.Columns || currentPosition.Y >= gameManager.gameMap.Rows)
                {
                    LogToFile("Fireball went out of bounds and disappeared.");
                    break; // Out of bounds
                }

                // Check if the fireball hits a wall using the tile type
                Tile tile = gameManager.gameMap.Tiles[currentPosition.Y, currentPosition.X];
                if (tile is NonWalkableTile)
                {
                    LogToFile("Fireball hit a wall and disappeared.");
                    break; // Fireball hits a wall
                }

                // Check if the fireball hits an enemy
                Enemy enemy = gameManager.Enemies.Find(e => e.GridPosition == currentPosition);
                if (enemy != null)
                {
                    enemy.TakeDamage(Damage);
                    LogToFile($"Fireball hit enemy at {currentPosition} for {Damage} damage!");

                    if (!enemy.IsAlive)
                    {
                        gameManager.MarkEnemyForRemoval(enemy); // Mark for removal if defeated
                        LogToFile($"Enemy at {currentPosition} defeated by Fireball.");
                    }
                    break; // Fireball hits an enemy
                }
            }

            gameManager.PlayerInventory.RemoveItem(this);
            gameManager.turnManager.EndPlayerTurn();
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