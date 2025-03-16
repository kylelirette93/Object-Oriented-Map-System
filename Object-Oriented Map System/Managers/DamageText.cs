using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Object_Oriented_Map_System.Managers
{
    public class DamageText
    {
        public string Text { get; private set; }
        public Vector2 Position { get; private set; }
        private Vector2 velocity;
        private float lifetime;
        private Color color;
        private SpriteFont font;

        public bool IsExpired => lifetime <= 0;

        public DamageText(string text, Vector2 position, SpriteFont font)
        {
            Text = text;
            Position = position;
            this.font = font;
            velocity = new Vector2(0, -1); // Move text upwards
            lifetime = 1.0f; // 1 second duration
            color = Color.Red;
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += velocity * 50f * deltaTime; // Move up over time
            lifetime -= deltaTime;
            color = new Color(color.R, color.G, color.B, (byte)(255 * (lifetime / 1.0f))); // Fade out effect
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsExpired)
            {
                spriteBatch.DrawString(font, Text, Position, color);
            }
        }
    }
}