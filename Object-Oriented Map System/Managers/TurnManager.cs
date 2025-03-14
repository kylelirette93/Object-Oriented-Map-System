using System;
using System.IO;
using Microsoft.Xna.Framework;
using Object_Oriented_Map_System.Entities;
using Object_Oriented_Map_System.Managers;

namespace Object_Oriented_Map_System.Managers
{
    public enum TurnState
    {
        PlayerTurn,
        EnemyTurn
    }

    public class TurnManager
    {
        public TurnState CurrentTurn { get; private set; }
        private GameManager gameManager;
        private int currentEnemyIndex = 0; // Track which enemy is taking its turn

        public TurnManager(GameManager manager)
        {
            gameManager = manager;
            CurrentTurn = TurnState.PlayerTurn; // Start with player turn
        }

        public void StartPlayerTurn() 
        {
            LogToFile("Starting Player Turn...");
            CurrentTurn = TurnState.PlayerTurn;
            gameManager.SetPlayerCanMove(true);
        }

        public bool IsPlayerTurn()
        {
            return CurrentTurn == TurnState.PlayerTurn;
        }

        public void EndPlayerTurn()
        {
            LogToFile("Player turn ended. Switching to Enemy Turn.");
            CurrentTurn = TurnState.EnemyTurn;
            currentEnemyIndex = 0;
            StartEnemyTurn();
        }

        private void StartEnemyTurn()
        {
            LogToFile("Enemy turn started. Processing enemy actions...");

            ProcessNextEnemy(0); // Start enemy actions, ensuring they move in sequence
        }

        private void ProcessNextEnemy(int index)
        {
            if (index < gameManager.Enemies.Count)
            {
                Enemy enemy = gameManager.Enemies[index];
                LogToFile($"Processing turn for Enemy at {enemy.GridPosition}...");

                // Enemy takes turn, then we process the next enemy after a delay
                enemy.TakeTurn(() =>
                {
                    ProcessNextEnemy(index + 1);
                });
            }
            else
            {
                EndEnemyTurn(); // All enemies finished, return to player turn
            }
        }

        private void EndEnemyTurn()
        {
            LogToFile("Enemy turn ended. Switching back to Player Turn.");
            CurrentTurn = TurnState.PlayerTurn;
            StartPlayerTurn();
        }

        private void LogToFile(string message)
        {
            string logPath = "debug_log.txt";
            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
    }
}