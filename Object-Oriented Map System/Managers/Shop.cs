using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Object_Oriented_Map_System.Entities;

namespace Object_Oriented_Map_System.Managers
{
    public class Shop
{
        public List<Item> purchasableItems = new List<Item>();
        Item selectedItem;
        KeyboardState previousKeyboardState;
        KeyboardState currentKeyboardState;
        public Point GridPosition { get; set; }
        public bool IsVisited { get { return isVisited; } }
        bool isVisited = false;
        Texture2D bombTexture;
        Texture2D healthPotionTexture;
        Texture2D fireballTexture;

        Inventory shopInventory = new Inventory();
        string shopText;
        SpriteFont shopFont;

        public Shop(Point gridPos)
        {
            GridPosition = gridPos;
        }

        public void LoadContent(ContentManager content)
        {
            if (content != null)
            {
                bombTexture = content.Load<Texture2D>("Bomb");
                healthPotionTexture = content.Load<Texture2D>("HealthPotion");
                fireballTexture = content.Load<Texture2D>("FireballScroll");
                shopFont = content.Load<SpriteFont>("DamageFont");
            }
        }

        public void Visit()
        {
            PopulateShop();
            isVisited = true;
        }
        public void SelectItem()
        {
            currentKeyboardState = Keyboard.GetState();
            if (currentKeyboardState != previousKeyboardState)
            {
                if (currentKeyboardState.IsKeyDown(Keys.NumPad1))
                {
                    // Select item 1.

                }
                if (currentKeyboardState.IsKeyDown(Keys.NumPad2))
                {
                    // Select item 2.
                }
                if (currentKeyboardState.IsKeyDown(Keys.NumPad3))
                {
                    // Select item 3.
                }
            }
            currentKeyboardState = previousKeyboardState;
        }


        public void PopulateShop()
        {
            // Display each available item in shop.
            Console.WriteLine("Visited shop! Yay...");
            purchasableItems.Add(new HealthPotion(healthPotionTexture, new Point(0, 3)));
            purchasableItems.Add(new BombItem(bombTexture, new Point(1, 3)));
            purchasableItems.Add(new FireballScroll(fireballTexture, new Point(2, 3)));
            foreach (var item in purchasableItems)
            {
                if (item != null)
                {
                    shopInventory.AddItem(item);
                }
            }
        }

        public void BuyItem(Item item, GameManager gameManager)
        {
           // handle currency and buying a selected item.
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            shopText = "Shop Inventory: \n [1]  [2]  [3]";
            Vector2 shopTextPos = new Vector2(10, 140);
            Vector2 shopInventoryPosition = new Vector2(10, 220);
            shopFont.Spacing = 3;

            spriteBatch.DrawString(shopFont, shopText, shopTextPos, Color.Black);
            shopInventory.Draw(spriteBatch, shopFont, shopInventoryPosition, GameManager.whiteTexture, 3);
        }
}
}
