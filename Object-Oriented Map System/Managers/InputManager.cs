using Microsoft.Xna.Framework;
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
    /// Input Manager handles keyboard inputs.
    /// </summary>
   public class InputManager
{
        private static readonly InputManager instance = new InputManager();

        // Singleton pattern for event bus.
        public static InputManager Instance
        {
            get
            {
                return instance;
            }
        }
        public KeyboardState PreviousKeyboardState { get { return previousKeyboardState; } set { previousKeyboardState = value; } }
        private KeyboardState previousKeyboardState;
        private KeyboardState currentKeyboardState;
        public List<(float TimeRemaining, Action Callback)> delayedActions = new List<(float, Action)>();
        Player player;

        public InputManager()
        {
            previousKeyboardState = Keyboard.GetState();
        }

        public void SetPlayer(Player player)
        {
            this.player = player;
        }

        public void ScheduleDelayedAction(float delay, Action action)
        {
            delayedActions.Add((delay, action));
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
