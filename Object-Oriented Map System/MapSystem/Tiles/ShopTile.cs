using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Object_Oriented_Map_System.MapSystem.Tiles
{
    public class ShopTile : Tile
    {
        public ShopTile(Texture2D texture, Vector2 position)
            : base(texture, position)
        {
            Walkable = false; //Prevent walking by default
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.White);
        }
    }
}
