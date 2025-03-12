using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Object_Oriented_Map_System.MapSystem.Tiles
{
    public class ExitTile : Tile
    {       
        public ExitTile(Texture2D texture, Vector2 position)
            : base(texture, position)
        {           
            Walkable = false; //Prevent walking by default
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.Green);
        }
    }
}