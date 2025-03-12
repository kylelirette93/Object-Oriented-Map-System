using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Object_Oriented_Map_System.MapSystem.Tiles
{
    public class OpenExitTile : ExitTile
    {
        public OpenExitTile(Texture2D texture, Vector2 position)
            : base(texture, position) 
        {
            Walkable = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Different color to indicate it's open (change as needed)
            spriteBatch.Draw(Texture, Position, Color.Yellow);
        }
    }
}