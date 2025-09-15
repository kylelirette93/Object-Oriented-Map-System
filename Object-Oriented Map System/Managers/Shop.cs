using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
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

        public Shop(Point gridPos)
        {
            GridPosition = gridPos;
        }

        public void Visit()
        {
            DisplayItems();
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


        public void DisplayItems()
        {
            // Display each available item in shop.
            Console.WriteLine("Visited shop! Yay...");
        }

        public void BuyItem(Item item, GameManager gameManager)
        {
           // handle currency and buying a selected item.
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var item in purchasableItems)
            {
                item.Draw(spriteBatch, 32, 32);
            }
        }
}
}
