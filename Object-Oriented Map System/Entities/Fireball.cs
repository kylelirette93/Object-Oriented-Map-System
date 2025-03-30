using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Object_Oriented_Map_System.Managers;

namespace Object_Oriented_Map_System.Entities
{
    public class Fireball
    {
        private Texture2D texture;
        private Point position;
        private Point direction;
        private GameManager gameManager;
        private int damage;

        public bool IsActive { get; private set; } = true;

        public Fireball(Texture2D texture, Point startPosition, Point direction, int damage, GameManager manager)
        {
            this.texture = texture;
            this.position = startPosition;
            this.direction = direction;
            this.damage = damage;
            this.gameManager = manager;
        }

        public void Update()
        {
            if (!IsActive) return;

            // Move the fireball
            position += direction;

            // Check if fireball hits a wall
            if (!gameManager.IsCellAccessible(position.X, position.Y))
            {
                IsActive = false;
                LogToFile("Fireball hit a wall and disappeared.");
                return;
            }

            // Check if fireball hits an enemy
            Enemy enemy = gameManager.Enemies.Find(e => e.GridPosition == position);
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                LogToFile($"Fireball hit enemy at {position} for {damage} damage!");

                // Create a damage text at the enemy's position
                Vector2 damageTextPosition = new Vector2(
                    enemy.GridPosition.X * gameManager.gameMap.TileWidth + gameManager.gameMap.TileWidth / 2,
                    enemy.GridPosition.Y * gameManager.gameMap.TileHeight
                );
                gameManager.AddDamageText($"-{damage}", damageTextPosition);

                if (!enemy.IsAlive)
                {
                    gameManager.MarkEnemyForRemoval(enemy); // Ensure enemy is marked for removal
                    LogToFile($"Enemy at {position} defeated by Fireball.");
                }

                IsActive = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch, int tileWidth, int tileHeight)
        {
            if (IsActive)
            {
                Vector2 worldPosition = new Vector2(position.X * tileWidth, position.Y * tileHeight);
                spriteBatch.Draw(texture, worldPosition, Color.White);
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