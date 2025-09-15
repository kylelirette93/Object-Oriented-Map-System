using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Object_Oriented_Map_System.Managers;

namespace Object_Oriented_Map_System.Entities
{
    public class BombItem : Item
    {
        public int Damage { get; private set; } = 2; // Damage dealt to enemies and the player

        public BombItem(Texture2D texture, Point gridPosition)
            : base(texture, gridPosition)
        {
        }

        public override void OnPickup(GameManager gameManager)
        {
            base.OnPickup(gameManager);
            gameManager.player.PlayerInventory.AddItem(this);
        }

        public override void Use(GameManager gameManager)
        {
            if (gameManager.player.PlayerHealth.IsAlive)
            {
                gameManager.EnterBombAimingMode(this);
                LogToFile("Player used Bomb. Choose a direction to throw it.");
            }
        }

        public void ThrowBomb(GameManager gameManager, Point direction)
        {
            Point startPosition = gameManager.player.PlayerGridPosition;

            // Create a BombProjectile and add it to the active list
            BombProjectile bombProjectile = new BombProjectile(gameManager.bombTexture, startPosition, direction, Damage, gameManager);
            gameManager.ActiveBombs.Add(bombProjectile);

            // Remove the bomb from inventory and end the player's turn
            gameManager.player.PlayerInventory.RemoveItem(this);
            gameManager.turnManager.EndPlayerTurn();
            LogToFile("Bomb thrown!");
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