using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Object_Oriented_Map_System.Managers;
using System.Collections.Generic;


namespace Object_Oriented_Map_System.QuestSystem
{
    /// <summary>
    /// Quest UI displays quests on screen.
    /// </summary>
    public class QuestUI
    {
        List<Quest> questsToDisplay = new List<Quest>();
        List<Quest> completedQuests = new List<Quest>();
        SpriteFont questFont;
        string questText;
        readonly Vector2 questTextPos = new Vector2(580, 140);
        readonly Vector2 completedQuestTextPos = new Vector2(565, 300);
        Vector2 yOffset = new Vector2(0, 40);
        public QuestUI(List<Quest> questList, SpriteFont font)
        {
            questsToDisplay = questList;
            questFont = font;
            // Subscribe to event to add completed quest to redraw list.
            EventBus.Instance.Subscribe<Quest>(EventType.AddCompletedQuest, AddCompletedQuest);
        }

        public void AddCompletedQuest(Quest quest)
        {
            if (!completedQuests.Contains(quest))
            {
                completedQuests.Add(quest);
            }
        }

        private void DrawQuestList(SpriteBatch spriteBatch, List<Quest> quests, Vector2 startPos, Color color)
        {
            Vector2 position = startPos;
            foreach (var quest in quests)
            {
                spriteBatch.DrawString(questFont, quest.Description, position, color);
                questFont.Spacing = 2;
                position += yOffset;
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (questsToDisplay.Count == 0)
            {
                questText = "Congrats!\n" + "You finished\n" + "the game!";
                spriteBatch.DrawString(questFont, questText, questTextPos, Color.Black);
            }
            else
            {
                spriteBatch.DrawString(questFont, "Active Quests:\n", questTextPos, Color.Black);
                DrawQuestList(spriteBatch, questsToDisplay, questTextPos + yOffset, Color.Black);

                spriteBatch.DrawString(questFont, "Finished Quests:\n", completedQuestTextPos, Color.LightGreen);
                DrawQuestList(spriteBatch, completedQuests, completedQuestTextPos + yOffset, Color.LightGreen);
            }
        }
    }
}
