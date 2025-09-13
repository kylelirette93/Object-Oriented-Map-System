using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Object_Oriented_Map_System.Entities;

namespace Object_Oriented_Map_System.Managers
{
    public class Shop
{
        public List<Item> purchasableItems = new List<Item>();
        Item selectedItem;

        public void SelectItem()
        {
            foreach (var item in purchasableItems)
            {
                // Handle selection of item, if they're displayed.
            }
        }

        public void DisplayItems()
        {
            
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
