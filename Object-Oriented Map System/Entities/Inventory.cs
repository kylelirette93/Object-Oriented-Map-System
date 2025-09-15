using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Object_Oriented_Map_System.Managers;

namespace Object_Oriented_Map_System.Entities
{
    public class Inventory
    {
        public List<Item> Items { get; private set; }
        private int maxSlots = 5;

        public Inventory()
        {
            Items = new List<Item>(maxSlots);
        }

        public bool AddItem(Item item)
        {
            if (Items.Count >= maxSlots)
            {
                return false; // Inventory is full
            }

            Items.Add(item);
            return true;
        }

        public void RemoveItem(Item item)
        {
            if (Items == null || Items.Count == 0)
            {
                LogToFile("Inventory is empty. No item to remove.");
                return;
            }

            if (Items.Contains(item))
            {
                Items.Remove(item);
                LogToFile($"Item removed: {item.GetType().Name}. Remaining items: {Items.Count}");
            }
            else
            {
                LogToFile($"Item not found in inventory: {item.GetType().Name}");
            }
        }

        public void UseItem(int index, GameManager gameManager)
        {
            // Validate index to prevent out of range error
            if (Items == null || Items.Count == 0)
            {
                LogToFile("Inventory is empty. No item to use.");
                return;
            }

            if (index < 0 || index >= Items.Count)
            {
                LogToFile($"Invalid inventory slot. Index: {index}, Items Count: {Items.Count}");
                return;
            }

            Item item = Items[index];
            LogToFile($"Using item: {item.GetType().Name} at index {index}");
            item.Use(gameManager);            
        }


        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, Texture2D whiteTexture)
        {
            for (int i = 0; i < maxSlots; i++)
            {
                // Draw inventory slots as rectangles
                Rectangle slotRect = new Rectangle((int)position.X + i * 40, (int)position.Y, 32, 32);

                // Draw a grey border
                spriteBatch.Draw(whiteTexture, new Rectangle(slotRect.X - 2, slotRect.Y - 2, 36, 36), Color.DarkGray);

                // Draw a white rectangle for the slot background
                spriteBatch.Draw(whiteTexture, slotRect, Color.LightGray);

                // Draw the item if it exists
                if (i < Items.Count)
                {
                    spriteBatch.Draw(Items[i].Texture, slotRect, Color.White);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, Texture2D whiteTexture, int maxSlots)
        {
            for (int i = 0; i < maxSlots; i++)
            {
                // Draw inventory slots as rectangles
                Rectangle slotRect = new Rectangle((int)position.X + i * 40, (int)position.Y, 32, 32);

                // Draw a grey border
                spriteBatch.Draw(whiteTexture, new Rectangle(slotRect.X - 2, slotRect.Y - 2, 36, 36), Color.DarkGray);

                // Draw a white rectangle for the slot background
                spriteBatch.Draw(whiteTexture, slotRect, Color.LightGray);

                // Draw the item if it exists
                if (i < Items.Count)
                {
                    spriteBatch.Draw(Items[i].Texture, slotRect, Color.White);
                }
            }
        }



        private void LogToFile(string message)
        {
            string logPath = "debug_log.txt";
            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
    }
}