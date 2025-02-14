using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Object_Oriented_Map_System.MapSystem.Tiles
{
    public class WalkableTile : Tile
    {
        public WalkableTile(Texture2D texture, Vector2 position)
            : base(texture, position)
        {
            Walkable = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.White);
        }
    }
}