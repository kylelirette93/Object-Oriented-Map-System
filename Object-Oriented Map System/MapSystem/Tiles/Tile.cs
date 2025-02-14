using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Object_Oriented_Map_System.MapSystem.Tiles
{
    public abstract class Tile
    {
        public Vector2 Position { get; protected set; }
        public Texture2D Texture { get; protected set; }
        public bool Walkable { get; protected set; }

        public Tile(Texture2D texture, Vector2 position)
        {
            Texture = texture;
            Position = position;
        }

        public abstract void Draw(SpriteBatch spriteBatch);
    }
}
