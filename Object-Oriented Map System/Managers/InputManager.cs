using Microsoft.Xna.Framework.Input;
using Object_Oriented_Map_System.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Object_Oriented_Map_System.Managers
{
    /// <summary>
    /// Input Manager handles keyboard inputs and interacts with turn manager.
    /// </summary>
   public class InputManager
{
        public KeyboardState PreviousKeyboardState { get { return previousKeyboardState; } set { previousKeyboardState = value; } }
        private KeyboardState previousKeyboardState;
        public List<(float TimeRemaining, Action Callback)> delayedActions = new List<(float, Action)>();
        Player player;

        public InputManager(Player player)
        {
            this.player = player;
            previousKeyboardState = Keyboard.GetState();
        }

        public void SetState(KeyboardState state)
        {
            previousKeyboardState = state;
        }

        private void HandleItemUsage(KeyboardState currentKeyboardState)
        {
            for (int i = 0; i < 5; i++)
            {
                Keys key = Keys.D1 + i; // Keys 1-5
                if (currentKeyboardState.IsKeyDown(key) && !!previousKeyboardState.IsKeyDown(key))
                {
                    // Ensure the index is within the inventory size
                    if (player.PlayerInventory.Items.Count > i)
                    {
                        // Use the item if it's present in the inventory
                        player.PlayerInventory.UseItem(i);
                    }
                    else
                    {
                        GameManager.Instance.LogToFile($"No item in slot {i + 1}.");
                    }
                }
            }
        }

        public void GetState(out KeyboardState state)
        {
           state = Keyboard.GetState();
        }
    }
}
