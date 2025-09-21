using Microsoft.Xna.Framework.Graphics;
namespace Object_Oriented_Map_System.QuestSystem

{
    /// <summary>
    /// Quest Manager loads quests and update their statees.
    /// </summary>
    public class QuestManager
{
        QuestTracker questTracker;
        QuestUI questUI;

        public QuestManager(SpriteFont font)
        {
            questTracker = new QuestTracker();
            LoadQuests();
            // Pass loaded quests to be tracked.
            questUI = new QuestUI(questTracker.ActiveQuests, font);
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
                Name = "Buy 3 Items",
                Type = QuestType.Buy3Items,
                Description = "Buy 3 Items",
                TargetCount = 3,
                IsCompleted = false
            });
        }

        public void Update()
        {
            questTracker.CheckProgression();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            questUI.Draw(spriteBatch);
        }
    }
}
