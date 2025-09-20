using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Object_Oriented_Map_System.QuestSystem
{
    public class QuestUI
{
        List<Quest> questsToDisplay = new List<Quest>();
        public QuestUI(List<Quest> questList)
        {
            questsToDisplay = questList;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Quest quest in questsToDisplay)
            {

            }

        }
}
}
