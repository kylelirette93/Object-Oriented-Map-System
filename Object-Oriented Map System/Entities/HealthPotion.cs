using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Object_Oriented_Map_System.Managers;

namespace Object_Oriented_Map_System.Entities
{
    public class HealthPotion : Item
    {
        public int HealAmount { get; private set; } = 2; // Heals for 2 HP

        public HealthPotion(Texture2D texture, Point gridPosition)
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
            // Heal the player and remove the item
            if (gameManager.PlayerHealth.IsAlive)
            {
                int healAmount = Math.Min(HealAmount, gameManager.PlayerHealth.MaxHealth - gameManager.PlayerHealth.CurrentHealth);

                if (healAmount > 0)
                {
                    gameManager.PlayerHealth.Heal(healAmount);
                    LogToFile($"Player used HealthPotion and healed {healAmount} HP.");
                    gameManager.PlayerInventory.RemoveItem(this);
                    gameManager.turnManager.EndPlayerTurn();
                }
                else
                {
                    LogToFile("Player is already at full health.");
                }
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