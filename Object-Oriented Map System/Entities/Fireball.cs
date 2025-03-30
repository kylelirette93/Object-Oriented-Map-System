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
        private bool isEnemyProjectile;

        public bool IsActive { get; private set; } = true;

        public Fireball(Texture2D texture, Point startPosition, Point direction, int damage, GameManager manager, bool isEnemyProjectile = false)
        {
            this.texture = texture;
            this.position = startPosition;
            this.direction = direction;
            this.damage = damage;
            this.gameManager = manager;
            this.isEnemyProjectile = isEnemyProjectile;
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

            if (isEnemyProjectile)
            {
                // Enemy fireball hits the player
                if (gameManager.PlayerGridPosition == position)
                {
                    gameManager.PlayerTakeDamage(damage);
                    Vector2 damageTextPosition = new Vector2(
                        position.X * gameManager.gameMap.TileWidth + gameManager.gameMap.TileWidth / 2,
                        position.Y * gameManager.gameMap.TileHeight
                    );
                    gameManager.AddDamageText($"-{damage}", damageTextPosition);
                    IsActive = false;
                    LogToFile($"Player hit by a Fireball for {damage} damage!");
                    return;
                }
            }
            else
            {
                // Player fireball hits an enemy
                Enemy enemy = gameManager.Enemies.Find(e => e.GridPosition == position);
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Vector2 damageTextPosition = new Vector2(
                        position.X * gameManager.gameMap.TileWidth + gameManager.gameMap.TileWidth / 2,
                        position.Y * gameManager.gameMap.TileHeight
                    );
                    gameManager.AddDamageText($"-{damage}", damageTextPosition);
                    IsActive = false;
                    LogToFile($"Fireball hit enemy at {position} for {damage} damage!");

                    if (!enemy.IsAlive)
                    {
                        gameManager.MarkEnemyForRemoval(enemy);
                        LogToFile($"Enemy at {position} defeated by Fireball.");
                    }
                    return;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, int tileWidth, int tileHeight)
        {
            if (IsActive)
            {
                Vector2 worldPosition = new Vector2(position.X * tileWidth, position.Y * tileHeight);
                Color fireballColor = isEnemyProjectile ? Color.Red : Color.White; // Red for enemy fireballs
                spriteBatch.Draw(texture, worldPosition, fireballColor);
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