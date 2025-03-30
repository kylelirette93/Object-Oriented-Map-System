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
                gameManager.LastAttackWasScroll = true; // Mark it as a scroll attack
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
            // Create a fireball object at the player's current position
            Point startPosition = gameManager.PlayerGridPosition;

            // Instantiate a Fireball and add it to the active list
            Fireball fireball = new Fireball(gameManager.fireballTexture, startPosition, direction, Damage, gameManager);
            gameManager.ActiveFireballs.Add(fireball);

            // Remove the scroll and end the player's turn
            gameManager.PlayerInventory.RemoveItem(this);
            gameManager.turnManager.EndPlayerTurn();
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