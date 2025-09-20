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
        Dictionary<EventType, List<Delegate>> argumentListeners = new Dictionary<EventType, List<Delegate>>();
        Dictionary<EventType, List<Action>> eventListeners = new Dictionary<EventType, List<Action>>();

        Dictionary<EventType, bool> eventInProgress = new();

        /// <summary>
        /// Subscribes to an event, and creates a list of listeners associated with it.
        /// </summary>
        /// <param name="eventType">The event type to add a listener to.</param>
        /// <param name="listener">The action associated with an event.</param>
        public void Subscribe<T>(EventType eventType, Action<T> listener)
        {
            // Check if the listener doesn't already contain the event type.
            if (!argumentListeners.ContainsKey(eventType))
            {
                // Create a new list of actions for that specific event.
                argumentListeners[eventType] = new List<Delegate>();
            }
            argumentListeners[eventType].Add(listener);
        }

        public void Subscribe(EventType eventType, Action listener)
        {
            if (!eventListeners.ContainsKey(eventType))
            {
                eventListeners[eventType] = new List<Action>();
            }
            eventListeners[eventType].Add(listener);
        }

        /// <summary>
        /// When a state change occurs, publish is called which checks for any 
        /// methods associated with the event type and invokes them.
        /// </summary>
        /// <param name="eventType"></param>
        public void Publish<T>(EventType eventType, T arg)
        {
            if (argumentListeners.TryGetValue(eventType, out var listeners))
            {
                foreach (var listener in listeners.Cast<Action<T>>())
                {
                    listener(arg);
                }
            }
        }

        public void Publish(EventType eventType)
        {
            if (eventListeners.ContainsKey(eventType))
            {
                foreach (var listener in eventListeners[eventType])
                {
                    listener.Invoke();
                }
            }
        }
}

    public enum EventType
    {
        KillEnemy,
        PickupItem,
        WaveCompleted,
        QuestCompleted
    }
}
