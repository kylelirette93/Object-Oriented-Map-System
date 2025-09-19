using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Object_Oriented_Map_System.Managers
{
    /// <summary>
    /// Event Bus class acts as a middle man. Allowing classes to interact with each other. 
    /// An event is subscribed to either in a constructor or initialize method.
    /// If an object is removed/destroyed, that object should unsubscribe, remove any listeners associated with the event that was subscribed to.
    /// An event is published on state change, calling whatever method is passed as a listener.
    /// </summary>
    public class EventBus
{
        // Ensure instance can only be assigned to once.
        private static readonly EventBus instance = new EventBus();

        // Singleton pattern for event bus.
        public static EventBus Instance
        {
            get
            {
                return instance;
            }
        }
        Dictionary<EventType, List<Delegate>> eventListeners = new Dictionary<EventType, List<Delegate>>();

        /// <summary>
        /// Subscribes to an event, and creates a list of listeners associated with it.
        /// </summary>
        /// <param name="eventType">The event type to add a listener to.</param>
        /// <param name="listener">The action associated with an event.</param>
        public void Subscribe<T>(EventType eventType, Action<T> listener)
        {
            // Check if the listener doesn't already contain the event type.
            if (!eventListeners.ContainsKey(eventType))
            {
                // Create a new list of actions for that specific event.
                eventListeners[eventType] = new List<Delegate>();
            }
            eventListeners[eventType].Add(listener);
        }

        /// <summary>
        /// Removes each listener from a specific event.
        /// </summary>
        /// <param name="eventType">The event to remove listeners from.</param>
        public void Unsubscribe(EventType eventType)
        {
            // Check the dictionary for which event to unsubscribe from.
            if (eventListeners.ContainsKey(eventType))
            {
                foreach (Action listener in eventListeners[eventType])
                {
                    // Remove all listeners from that specific event.
                    eventListeners[eventType].Remove(listener);
                }
            }
        }

        /// <summary>
        /// When a state change occurs, publish is called which checks for any 
        /// methods associated with the event type and invokes them.
        /// </summary>
        /// <param name="eventType"></param>
        public void Publish<T>(EventType eventType, T arg)
        {
            if (eventListeners.TryGetValue(eventType, out var listeners))
            {
                foreach (var listener in listeners.Cast<Action<T>>())
                {
                    listener(arg);
                }
            }
        }
}

    public enum EventType
    {
        EarnCash,
        PickupItem
    }
}
