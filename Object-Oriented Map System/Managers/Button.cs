using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Button
{
    public Rectangle Bounds;
    public string Text;
    public SpriteFont Font;
    public Color TextColor = Color.Black;
    public Color BackgroundColor = Color.LightGray;
    public Color HoverColor = Color.Gray;

    private Texture2D texture;
    private bool isHovered;

    public Button(GraphicsDevice graphicsDevice, Rectangle bounds, string text, SpriteFont font)
    {
        Bounds = bounds;
        Text = text;
        Font = font;

        // Create a 1x1 white texture for button background
        texture = new Texture2D(graphicsDevice, 1, 1);
        texture.SetData(new[] { Color.White });
    }

    public void Update(MouseState mouseState)
    {
        isHovered = Bounds.Contains(mouseState.Position);
    }

    public bool IsClicked(MouseState currentMouse, MouseState previousMouse)
    {
        return isHovered &&
               currentMouse.LeftButton == ButtonState.Pressed &&
               previousMouse.LeftButton == ButtonState.Released;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Color currentColor = isHovered ? HoverColor : BackgroundColor;
        spriteBatch.Draw(texture, Bounds, currentColor);

        Vector2 textSize = Font.MeasureString(Text);
        Vector2 textPos = new Vector2(
            Bounds.X + (Bounds.Width - textSize.X) / 2,
            Bounds.Y + (Bounds.Height - textSize.Y) / 2
        );

        spriteBatch.DrawString(Font, Text, textPos, TextColor);
    }
}