using Object_Oriented_Map_System.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Object_Oriented_Map_System.QuestSystem
{
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

        public bool CheckProgress(int progress)
        {
            return progress >= targetCount;
        }

        public void Complete()
        {
            IsCompleted = true;
            IsActive = false;
            Console.WriteLine("Quest completed: " + Name);
        }
}
}
