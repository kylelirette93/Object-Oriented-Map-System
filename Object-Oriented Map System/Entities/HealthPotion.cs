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
            price = 5;
        }

        public override void OnPickup()
        {
            base.OnPickup();
        }

        public override void Use()
        {
            // Heal the player and remove the item
            if (GameManager.Instance.player.PlayerHealth.IsAlive)
            {
                int healAmount = Math.Min(HealAmount, GameManager.Instance.player.PlayerHealth.MaxHealth - GameManager.Instance.player.PlayerHealth.CurrentHealth);

                if (healAmount > 0)
                {
                    GameManager.Instance.player.PlayerHealth.Heal(healAmount);
                    LogToFile($"Player used HealthPotion and healed {healAmount} HP.");
                    GameManager.Instance.player.PlayerInventory.RemoveItem(this);
                    TurnManager.Instance.EndPlayerTurn();
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