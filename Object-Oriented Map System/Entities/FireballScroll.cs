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
            price = 7;
        }

        public override void OnPickup()
        {
            base.OnPickup();
        }

        public override void Use()
        {
            if (GameManager.Instance.player.PlayerHealth.IsAlive)
            {
                GameManager.Instance.LastAttackWasScroll = true; // Mark it as a scroll attack
                // Trigger the aiming mode for Fireball
                GameManager.Instance.EnterFireballAimingMode(this);
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
            // Create a fireball object at the player's current position
            Point startPosition = GameManager.Instance.player.PlayerGridPosition;

            // Instantiate a Fireball and add it to the active list
            Fireball fireball = new Fireball(GameManager.Instance.fireballTexture, startPosition, direction, Damage, gameManager);
            GameManager.Instance.ActiveFireballs.Add(fireball);

            // Remove the scroll and end the player's turn
            GameManager.Instance.player.PlayerInventory.RemoveItem(this);
            TurnManager.Instance.EndPlayerTurn();
            LogToFile("Fireball launched!");
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