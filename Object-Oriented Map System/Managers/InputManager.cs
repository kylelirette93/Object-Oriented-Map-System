using Microsoft.Xna.Framework.Input;
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

        public InputManager()
        {
            previousKeyboardState = Keyboard.GetState();
        }

        public void SetState(KeyboardState state)
        {
            previousKeyboardState = state;
        }

        public void GetState(out KeyboardState state)
        {
           state = Keyboard.GetState();
        }
    }
}
