using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Object_Oriented_Map_System.Managers;

namespace Object_Oriented_Map_System.Entities
{
    public class LightningScroll : Item
    {
        public int Damage { get; private set; } = 2; // Damage all enemies for 2 HP

        public LightningScroll(Texture2D texture, Point gridPosition)
            : base(texture, gridPosition)
        {
            price = 10;
        }

        public override void OnPickup()
        {
            base.OnPickup();
        }

        public override void Use()
        {
            if (gameManager.player.PlayerHealth.IsAlive)
            {
                gameManager.LastAttackWasScroll = true; // Mark as a scroll attack
                // Deal damage to all enemies on the map
                foreach (var enemy in gameManager.Enemies)
                {
                    enemy.TakeDamage(Damage);

                    // Visual feedback with damage numbers
                    Vector2 damageTextPosition = new Vector2(
                        enemy.GridPosition.X * gameManager.gameMap.TileWidth + gameManager.gameMap.TileWidth / 2,
                        enemy.GridPosition.Y * gameManager.gameMap.TileHeight
                    );
                    gameManager.AddDamageText($"-{Damage}", damageTextPosition);

                    // Remove if enemy dies
                    /*if (!enemy.IsAlive)
                    {
                        gameManager.MarkEnemyForRemoval(enemy);
                        LogToFile($"Enemy at {enemy.GridPosition} defeated by Lightning Scroll.");
                    }
                    else
                    {
                        LogToFile($"Enemy at {enemy.GridPosition} took {Damage} damage from Lightning Scroll.");
                    }*/
                }

                // Remove scroll from inventory and end the turn
                gameManager.player.PlayerInventory.RemoveItem(this);
                TurnManager.Instance.EndPlayerTurn();
                LogToFile("Player used Lightning Scroll. All visible enemies were struck!");
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
