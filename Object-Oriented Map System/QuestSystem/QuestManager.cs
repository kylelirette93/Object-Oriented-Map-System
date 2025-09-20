using System.Collections.Generic;
namespace Object_Oriented_Map_System.QuestSystem

{
    internal class QuestManager
{
        QuestTracker questTracker = new QuestTracker();

        public QuestManager()
        {
            LoadQuests();
        }

        public void LoadQuests()
        {
            questTracker.ActiveQuests.Add(new Quest
            {
                Name = "Kill 5 Enemies",
                Type = QuestType.KillEnemies,
                Description = "Kill 5 Enemies",
                TargetCount = 5,
                IsCompleted = false
            });
            questTracker.ActiveQuests.Add(new Quest
            {
                Name = "Beat 5 Waves",
                Type = QuestType.Beat5Waves,
                Description = "Beat 5 Waves",
                TargetCount = 5,
                IsCompleted = false
            });
            questTracker.ActiveQuests.Add(new Quest
            {
                Name = "Win the Game",
                Type = QuestType.QuestCompleted,
                Description = "Win the game",
                TargetCount = 2,
                IsCompleted = false
            });
        }

        public void Update()
        {
            questTracker.CheckProgression();
        }
    }
}
