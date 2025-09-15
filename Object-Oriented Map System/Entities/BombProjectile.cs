using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Object_Oriented_Map_System.Managers;

namespace Object_Oriented_Map_System.Entities
{
    public class BombProjectile
    {
        private Texture2D texture;
        private Point position;
        private Point direction;
        private GameManager gameManager;
        private int damage;
        public bool IsActive { get; private set; } = true;

        public BombProjectile(Texture2D texture, Point startPosition, Point direction, int damage, GameManager manager)
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

            // Move the bomb
            position += direction;

            // Check if bomb hits a wall
            if (!gameManager.IsCellAccessible(position.X, position.Y))
            {
                Explode();
                return;
            }

            // Check if bomb hits an enemy
            Enemy enemy = gameManager.Enemies.Find(e => e.GridPosition == position);
            if (enemy != null)
            {
                Explode();
                return;
            }
        }

        private void Explode()
        {
            LogToFile($"Bomb exploded at {position}!");

            List<Point> explosionTiles = new List<Point>();

            // Collect all tiles in a proper 1-tile radius (3x3 centered on explosion)
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Point explosionPoint = new Point(position.X + dx, position.Y + dy);

                    // Ensure the explosion stays within map bounds
                    if (explosionPoint.X >= 0 && explosionPoint.X < gameManager.gameMap.Columns &&
                        explosionPoint.Y >= 0 && explosionPoint.Y < gameManager.gameMap.Rows)
                    {
                        explosionTiles.Add(explosionPoint);

                        // Damage enemies
                        Enemy enemy = gameManager.Enemies.Find(e => e.GridPosition == explosionPoint);
                        if (enemy != null)
                        {
                            enemy.TakeDamage(damage);
                            Vector2 damageTextPosition = new Vector2(
                                enemy.GridPosition.X * gameManager.gameMap.TileWidth + gameManager.gameMap.TileWidth / 2,
                                enemy.GridPosition.Y * gameManager.gameMap.TileHeight
                            );
                            gameManager.AddDamageText($"-{damage}", damageTextPosition);
                            LogToFile($"Enemy at {explosionPoint} took {damage} damage from the bomb!");

                            if (!enemy.IsAlive)
                            {
                                gameManager.MarkEnemyForRemoval(enemy);
                            }
                        }

                        // Damage player if within range
                        if (explosionPoint == gameManager.player.PlayerGridPosition)
                        {
                            gameManager.PlayerTakeDamage(damage);
                            Vector2 damageTextPosition = new Vector2(
                                gameManager.player.PlayerGridPosition.X * gameManager.gameMap.TileWidth + gameManager.gameMap.TileWidth / 2,
                                gameManager.player.PlayerGridPosition.Y * gameManager.gameMap.TileHeight
                            );
                            gameManager.AddDamageText($"-{damage}", damageTextPosition);
                            LogToFile($"Player took {damage} damage from the bomb!");
                        }
                    }
                }
            }

            // Trigger explosion visuals
            gameManager.TriggerExplosionEffect(explosionTiles);

            IsActive = false;
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
