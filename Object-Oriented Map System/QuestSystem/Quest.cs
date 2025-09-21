using Object_Oriented_Map_System.Managers;
using System;

namespace Object_Oriented_Map_System.QuestSystem
{
    /// <summary>
    /// Quest class holds simple data for a specific quest.
    /// </summary>
    public class Quest
{
        public string Name { get; set; }
        public string Description { get { return description; } set { description = value; } }
        string description;
        public int TargetCount { get { return targetCount; } set { targetCount = value; } }
        int targetCount;
        public QuestType Type { get; set; }
        public bool IsCompleted { get { return isCompleted; } set { isCompleted = value; } }
        bool isCompleted = false;
        public bool IsActive { get { return isActive; } set { isActive = value; } }
        bool isActive = true;

        /// <summary>
        /// Checks progress of a quest, used for determining if quest is complete.
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        public bool CheckProgress(int progress)
        {
            return progress >= targetCount;
        }

        /// <summary>
        /// Completes a quest, changing its state.
        /// </summary>
        public void Complete()
        {
            IsCompleted = true;
            IsActive = false;
            Console.WriteLine("Quest completed: " + Name);
            // Publish event to notify quest tracker that its completed.
            EventBus.Instance.Publish<Quest>(EventType.AddCompletedQuest, this);
        }
}
}
