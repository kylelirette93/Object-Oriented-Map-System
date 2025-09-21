using Object_Oriented_Map_System.Managers;
using System.Collections.Generic;


namespace Object_Oriented_Map_System.QuestSystem
{
    /// <summary>
    /// Quest Tracker manages active quests and tracks their progression.
    /// </summary>
    public class QuestTracker
{
        int enemiesKilled;
        int wavesCompleted;
        int itemsBought;

        public List<Quest> ActiveQuests { get { return activeQuests; } set { activeQuests = value; } }
        List<Quest> activeQuests = new List<Quest>();

        public QuestTracker()
        {
            // Default values.
            enemiesKilled = 0;
            wavesCompleted = 0;
            itemsBought = 0;

            // Subscribe to events to progress quests.
            EventBus.Instance.Subscribe<int>(EventType.KillEnemy, OnEnemyKilled);
            EventBus.Instance.Subscribe<int>(EventType.WaveCompleted, OnWaveCompleted);
            EventBus.Instance.Subscribe<int>(EventType.BuyItem, OnBuyItem);
        }

        /// <summary>
        /// Checks progression of all active quests and completes them if conditions are met.
        /// </summary>
        public void CheckProgression()
        {
            // Loop backwards to safely remove completed quests before next iteration.
            for (int i = ActiveQuests.Count - 1; i >= 0; i--) 
            {
                var quest = ActiveQuests[i];

                if (quest.IsCompleted) continue;

                // Determines stat based on quest type.
                int stat = quest.Type switch
                {
                    QuestType.KillEnemies => enemiesKilled,
                    QuestType.ReachWave5 => wavesCompleted,
                    QuestType.Buy3Items => itemsBought,
                    _ => 0
                };

                // Check if quest condition is met.
                if (quest.CheckProgress(stat))
                {
                    quest.Complete();
                    ActiveQuests.RemoveAt(i);
                }
            }
        }

        private void OnEnemyKilled(int enemiesKilled)
        {
            this.enemiesKilled += enemiesKilled;
        }

        private void OnWaveCompleted(int wavesCompleted)
        {
            this.wavesCompleted = wavesCompleted;
        }

        private void OnBuyItem(int itemsBought)
        {
            this.itemsBought += itemsBought;
        }
    }
}

public enum QuestType
{
    KillEnemies,
    ReachWave5,
    Buy3Items
}
