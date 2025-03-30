using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Object_Oriented_Map_System.Entities
{
    public class ExplosionEffect
    {
        private List<Point> affectedTiles;
        private float duration;
        private float elapsedTime;
        private Color flashColor;

        public bool IsActive => elapsedTime < duration;

        public ExplosionEffect(List<Point> tiles, float duration = 0.5f)
        {
            affectedTiles = tiles;
            this.duration = duration;
            elapsedTime = 0f;
            flashColor = Color.OrangeRed * 0.7f; // Semi-transparent explosion color
        }

        public void Update(GameTime gameTime)
        {
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D whiteTexture, int tileWidth, int tileHeight)
        {
            if (!IsActive) return;

            foreach (var tile in affectedTiles)
            {
                Vector2 worldPosition = new Vector2(tile.X * tileWidth, tile.Y * tileHeight);
                spriteBatch.Draw(whiteTexture, worldPosition, null, flashColor, 0f, Vector2.Zero, new Vector2(tileWidth, tileHeight), SpriteEffects.None, 0f);
            }
        }
    }
}