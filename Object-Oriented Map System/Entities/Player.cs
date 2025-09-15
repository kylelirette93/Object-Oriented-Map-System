using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;


namespace Object_Oriented_Map_System.Entities
{
    public class Player
    {
        private Texture2D playerTexture;
        public Vector2 PlayerPosition { get { return playerPosition; } set { playerPosition = value; } }
        Vector2 playerPosition;
        private Point playerGridPosition;
        public Point PlayerGridPosition { get { return playerGridPosition; } set { playerGridPosition = value; } }
        public Action OnPlayerDeath;
        public bool PlayerCanMove { get { return playerCanMove; } set { playerCanMove = value; } }
        private bool playerCanMove = false; //  Track if player can move
        public Inventory PlayerInventory { get; private set; }
        public HealthComponent PlayerHealth { get; private set; }
        public Player(ContentManager contentManager)
        {
            playerTexture = contentManager.Load<Texture2D>("player");
            PlayerInventory = new Inventory();

            PlayerHealth = new HealthComponent(5); // Set player health to 5
        }



        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(playerTexture, playerPosition, null, Color.White * 1f, 0f,
                new Vector2(playerTexture.Width / 2, playerTexture.Height / 2),
                Vector2.One, SpriteEffects.None, 0f);
        }
}
}
