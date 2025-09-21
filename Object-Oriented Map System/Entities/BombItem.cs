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
            price = 6;
            name = "Bomb";
        }

        public override void OnPickup()
        {
            base.OnPickup();
        }

        public override void Use()
        {
            if (GameManager.Instance.player.PlayerHealth.IsAlive)
            {
                GameManager.Instance.EnterBombAimingMode(this);
                LogToFile("Player used Bomb. Choose a direction to throw it.");
            }
        }

        public void ThrowBomb(Point direction)
        {
            Point startPosition = GameManager.Instance.player.PlayerGridPosition;

            // Create a BombProjectile and add it to the active list
            BombProjectile bombProjectile = new BombProjectile(GameManager.Instance.bombTexture, startPosition, direction, Damage);
            GameManager.Instance.ActiveBombs.Add(bombProjectile);

            // Remove the bomb from inventory and end the player's turn
            GameManager.Instance.player.PlayerInventory.RemoveItem(this);
            TurnManager.Instance.EndPlayerTurn();
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