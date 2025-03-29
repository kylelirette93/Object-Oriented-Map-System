using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Object_Oriented_Map_System.Managers;

namespace Object_Oriented_Map_System.Entities
{
    public abstract class Item
    {
        public Texture2D Texture { get; protected set; }
        public Point GridPosition { get; protected set; }
        public bool IsPickedUp { get; private set; } = false;

        protected Item(Texture2D texture, Point gridPosition)
        {
            Texture = texture;
            GridPosition = gridPosition;
        }

        public virtual void OnPickup(GameManager gameManager)
        {
            IsPickedUp = true;
        }

        public virtual void Use(GameManager gameManager)
        {
            // Default behavior for items with no special effect
            
        }

        public virtual void Draw(SpriteBatch spriteBatch, int tileWidth, int tileHeight)
        {
            if (!IsPickedUp)
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