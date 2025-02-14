using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Object_Oriented_Map_System.MapSystem.Tiles
{
    public class ExitTile : Tile
    {
        public ExitTile(Texture2D texture, Vector2 position)
            : base(texture, position)
        {
            // Exit tiles can be considered walkable (for triggering a map change)
            Walkable = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.Green);
        }
    }
}