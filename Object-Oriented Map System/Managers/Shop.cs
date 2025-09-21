using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Object_Oriented_Map_System.Entities;

namespace Object_Oriented_Map_System.Managers
{
    /// <summary>
    /// Shop class that allows the player to purchase items, adding it to their inventory. 
    /// </summary>
    public class Shop
{
        List<Item> purchasableItems = new List<Item>();
        KeyboardState previousKeyboardState;
        KeyboardState currentKeyboardState;
        public Point GridPosition { get; private set; }
        public bool IsVisited { get { return isVisited; } }
        bool isVisited = false;
        Texture2D bombTexture;
        Texture2D healthPotionTexture;
        Texture2D fireballTexture;

        Inventory shopInventory = new Inventory(true);
        string shopText;
        string priceText;
        SpriteFont shopFont;
        Player player;

        public Shop(Point gridPos)
        {
            // Store's grid position of shop in world.
            GridPosition = gridPos;
        }

        public void LoadContent(ContentManager content)
        {
            // Load textures and font for shop.
            if (content != null)
            {
                bombTexture = content.Load<Texture2D>("Bomb");
                healthPotionTexture = content.Load<Texture2D>("HealthPotion");
                fireballTexture = content.Load<Texture2D>("FireballScroll");
                shopFont = content.Load<SpriteFont>("DamageFont");
                PopulateShop();
            }
        }

        /// <summary>
        /// Visiting the shop passes a reference to player, so their specific inventory can be accessed.
        /// </summary>
        /// <param name="player"></param>
        public void Visit(Player player)
        {
            // Pass reference to player to adjust inventory.
            this.player = player;
            isVisited = true;
        }

        public void Leave()
        {
            isVisited = false;
        }

        public void Update(GameTime gameTime)
        {
            // If the shop has been visited, allow player to select items.
            if (player != null && isVisited)
            {
                SelectFromItems();
            }
        }

        /// <summary>
        /// Select from items, gets the key pressed and buys corresponding item.
        /// </summary>
        public void SelectFromItems()
        {
            currentKeyboardState = Keyboard.GetState();
            if (player != null)
            {
                if (currentKeyboardState.IsKeyDown(Keys.D1) && previousKeyboardState.IsKeyUp(Keys.D1))
                {
                    // Add the first item to the player's inventory.
                    BuyItem(shopInventory.GetItem(0));
                }
                else if (currentKeyboardState.IsKeyDown(Keys.D2) && previousKeyboardState.IsKeyUp(Keys.D2))
                {
                    // Add the second item to the player's inventory.
                    BuyItem(shopInventory.GetItem(1));
                }
                else if (currentKeyboardState.IsKeyDown(Keys.D3) && previousKeyboardState.IsKeyUp(Keys.D3))
                {
                    // Add the third item to the player's inventory.
                    BuyItem(shopInventory.GetItem(2));
                }
                previousKeyboardState = currentKeyboardState;
            }           
        }

        /// <summary>
        /// Populates the shop with items the player can buy.
        /// </summary>
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
                    item.IsPickedUp = true;
                    shopInventory.AddShopItems(purchasableItems);
                }
            }
        }

        /// <summary>
        /// Adds the item purchased to player's inventory.
        /// </summary>
        /// <param name="item">The item purchased.</param>
        public void BuyItem(Item item)
        {
            // Check if player has enough money to buy the item.
            if (player.PlayerInventory.Currency >= item.Price)
            {
                player.PlayerInventory.Currency -= item.Price;
                player.PlayerInventory.AddItem(item);
                // Publish an event when item is bought.
                EventBus.Instance.Publish(EventType.BuyItem, 1);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            shopText = "Shop Inventory: \n [1]  [2]  [3]";
            // Display items with names and prices.
            priceText =  shopInventory.GetItem(0).Name + " $" + shopInventory.GetItem(0).Price + "\n" +
            shopInventory.GetItem(1).Name + " $" + shopInventory.GetItem(1).Price + "\n" +
            shopInventory.GetItem(2).Name + " $" + shopInventory.GetItem(2).Price;

            // Positioning of text and shop inventory.
            Vector2 shopTextPos = new Vector2(10, 140);
            Vector2 priceTextPos = new Vector2(10, 260);
            Vector2 shopInventoryPosition = new Vector2(10, 220);
            shopFont.Spacing = 2;

            // Drawing info and shop inventory to screen.
            spriteBatch.DrawString(shopFont, shopText, shopTextPos, Color.Black);
            spriteBatch.DrawString(shopFont, priceText, priceTextPos, Color.Goldenrod);
            shopInventory.Draw(spriteBatch, shopFont, shopInventoryPosition, GameManager.whiteTexture, 3);
        }
}
}
