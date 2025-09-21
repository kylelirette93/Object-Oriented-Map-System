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
        public bool IsPickedUp { get; set; } = false;

        // Kyle - Added item price, to handle buying items with shop system.
        public int Price { get { return price; } }
        protected int price;

        public string Name { get { return name; } }
        protected string name;
        protected GameManager gameManager;

        protected Item(Texture2D texture, Point gridPosition)
        {
            Texture = texture;
            GridPosition = gridPosition;
            gameManager = GameManager.Instance;
        }
        public virtual void OnPickup()
        {
            IsPickedUp = true;
            EventBus.Instance.Publish(EventType.PickupItem, this);
        }

        public virtual void Use()
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
    }
}