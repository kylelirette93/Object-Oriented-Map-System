using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Object_Oriented_Map_System.Managers;
using Object_Oriented_Map_System.MapSystem;
using Object_Oriented_Map_System.MapSystem.Tiles;

namespace Object_Oriented_Map_System.Entities
{
    public class RangedEnemy : Enemy
    {
        public int FireballDamage { get; private set; } = 1;

        public RangedEnemy(Texture2D texture, Point gridPosition, Map map, GameManager gameManager)
            : base(texture, gridPosition, map, gameManager)
        {
        }

        public override void TakeTurn(Action onComplete)
        {
            if (!IsAlive)
            {
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

            if (CanSeePlayer())
            {
                ShootProjectile();
                onComplete?.Invoke();
            }
            else
            {
                base.TakeTurn(onComplete); // Fallback to movement
            }

            onComplete?.Invoke();
        }

        private bool CanSeePlayer()
        {
            Point playerPosition = gameManager.player.PlayerGridPosition;

            // Check if the player is on the same row or column
            if (GridPosition.X != playerPosition.X && GridPosition.Y != playerPosition.Y)
                return false;

            // Perform a simple line-of-sight check
            Point direction = Point.Zero;
            if (GridPosition.X == playerPosition.X)
            {
                direction.Y = Math.Sign(playerPosition.Y - GridPosition.Y);
            }
            else
            {
                direction.X = Math.Sign(playerPosition.X - GridPosition.X);
            }

            Point current = GridPosition + direction;
            while (current != playerPosition)
            {
                if (!gameManager.IsCellAccessible(current.X, current.Y))
                    return false;
                current += direction;
            }

            return true;
        }

        private void ShootProjectile()
        {
            Point direction = Point.Zero;
            Point playerPosition = gameManager.player.PlayerGridPosition;

            if (GridPosition.X == playerPosition.X)
            {
                direction.Y = Math.Sign(playerPosition.Y - GridPosition.Y);
            }
            else
            {
                direction.X = Math.Sign(playerPosition.X - GridPosition.X);
            }

            Fireball fireball = new Fireball(gameManager.fireballTexture, GridPosition, direction, FireballDamage, gameManager, isEnemyProjectile: true);
            gameManager.ActiveFireballs.Add(fireball);
            LogToFile($"RangedEnemy fired a fireball from {GridPosition} towards {direction}");
        }

        public override void Die()
        {
            base.Die();
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