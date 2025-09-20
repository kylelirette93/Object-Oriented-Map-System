using Object_Oriented_Map_System.Managers;
using System.Collections.Generic;


namespace Object_Oriented_Map_System.QuestSystem
{
    public class QuestTracker
{
        int enemiesKilled;
        int wavesCompleted;
        int questsCompleted;

        public List<Quest> ActiveQuests { get { return activeQuests; } set { activeQuests = value; } }
        List<Quest> activeQuests = new List<Quest>();

        public QuestTracker()
        {
            enemiesKilled = 0;
            wavesCompleted = 0;
            questsCompleted = 0;

            // Subscribe to events.
            EventBus.Instance.Subscribe<int>(EventType.KillEnemy, OnEnemyKilled);
            EventBus.Instance.Subscribe<int>(EventType.WaveCompleted, OnWaveCompleted);
            EventBus.Instance.Subscribe<int>(EventType.QuestCompleted, OnQuestComplete);
        }

        public void CheckProgression()
        {
            for (int i = ActiveQuests.Count - 1; i >= 0; i--) 
            {
                var quest = ActiveQuests[i];

                if (quest.IsCompleted) continue;

                int stat = quest.Type switch
                {
                    QuestType.KillEnemies => enemiesKilled,
                    QuestType.Beat5Waves => wavesCompleted,
                    QuestType.QuestCompleted => questsCompleted,
                    _ => 0
                };

                if (quest.CheckProgress(stat))
                {
                    quest.Complete();
                    ActiveQuests.RemoveAt(i);
                }
            }
        }
        private void OnEnemyKilled(int count)
        {
            enemiesKilled += count;
        }

        private void OnWaveCompleted(int wavesCompleted)
        {
            this.wavesCompleted = wavesCompleted;
        }

        private void OnQuestComplete(int questsCompleted)
        {
            this.questsCompleted += questsCompleted;
        }
    }
}

public enum QuestType
{
    KillEnemies,
    Beat5Waves,
    QuestCompleted
}
