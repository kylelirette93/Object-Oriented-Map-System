using System;
using System.Collections.Generic;
using System.Linq;

namespace Object_Oriented_Map_System.Managers
{
    /// <summary>
    /// Event Bus class acts as a middle man. Allowing classes to interact with each other. 
    /// An event should be subscribed to either in a constructor or initialize method.
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
        // One dictionary for events with arguments, one for events without arguments.
        Dictionary<EventType, List<Delegate>> argumentListeners = new Dictionary<EventType, List<Delegate>>();
        Dictionary<EventType, List<Action>> eventListeners = new Dictionary<EventType, List<Action>>();


        /// <summary>
        /// Subscribe to an event by passing in the event type and the method to call when that event is published.
        /// </summary>
        /// <param name="eventType">The event type to subscribe to.</param>
        /// <param name="listener">A callback method that must accept a single parameter of type specified when publishing.</param>
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

        /// <summary>
        /// Subscribe to an event without aguments by simply passing in event type and method call.
        /// </summary>
        /// <param name="eventType">The event type to subscribe to.</param>
        /// <param name="listener">The method call.</param>
        public void Subscribe(EventType eventType, Action listener)
        {
            if (!eventListeners.ContainsKey(eventType))
            {
                eventListeners[eventType] = new List<Action>();
            }
            eventListeners[eventType].Add(listener);
        }

       /// <summary>
       /// A generic method to publish an event with an argument. It retrieves a list of delegates for given event type in dictionary
       /// casts them to the correct action and invokes each listener with argument.
       /// </summary>
       /// <typeparam name="T">Data type for the parameter, declared when publishing.</typeparam>
       /// <param name="eventType">The event type to publish.</param>
       /// <param name="arg">Argument of type T.</param>
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

        /// <summary>
        /// Publish an event without arguments.
        /// </summary>
        /// <param name="eventType">Event type to publish.</param>
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
        QuestCompleted,
        AddCompletedQuest,
        BuyItem
    }
}
