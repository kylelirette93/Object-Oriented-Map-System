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

        // ---------------------------- PLAYER TURN MANAGEMENT ----------------------------

        public void StartPlayerTurn()
        {
            //LogToFile("Starting Player Turn...");
            CurrentTurn = TurnState.PlayerTurn;
            gameManager.SetPlayerCanMove(true);
        }

        public void EndPlayerTurn()
        {
            //LogToFile("Player turn ended. Switching to Enemy Turn.");
            gameManager.LastAttackWasScroll = false; // Reset after the turn
            gameManager.SetPlayerCanMove(false);
            CurrentTurn = TurnState.EnemyTurn;

            if (gameManager.Enemies.Count == 0)
            {
                LogToFile("No enemies to process. Returning to Player Turn.");
                EndEnemyTurn();
                return;
            }

            ProcessNextEnemy(0); 
        }

        public bool IsPlayerTurn() => CurrentTurn == TurnState.PlayerTurn;
        public bool IsEnemyTurn() => CurrentTurn == TurnState.EnemyTurn;

        // ---------------------------- ENEMY TURN MANAGEMENT ----------------------------

        private void StartEnemyTurn()
        {
            //LogToFile("Enemy turn started. Processing enemy actions...");

            if (gameManager.Enemies.Count == 0)
            {
                //LogToFile("No enemies to process. Returning to Player Turn.");
                EndEnemyTurn();
                return;
            }

            //LogToFile($"Total enemies in turn order: {gameManager.Enemies.Count}");
            ProcessNextEnemy(0); // Begin enemy turns sequentially
        }

        private void ProcessNextEnemy(int index)
        {
            if (index >= gameManager.Enemies.Count)
            {
                //LogToFile("All enemies have moved. Switching back to Player Turn.");
                EndEnemyTurn();
                return;
            }

            // Ensure we are only processing enemies during EnemyTurn
            if (CurrentTurn != TurnState.EnemyTurn)
            {
                //LogToFile($"ERROR: Attempting to process enemy {index} while not in EnemyTurn!");
                return;
            }

            // Get enemy reference
            Enemy enemy = gameManager.Enemies[index];

            // Ensure enemy does NOT move on PlayerTurn
            if (gameManager.turnManager.IsPlayerTurn())
            {
                //LogToFile($"ERROR: Enemy {index} tried to move during Player Turn! Aborting.");
                return;
            }

            //LogToFile($"Processing turn for Enemy at {enemy.GridPosition}...");

            if (IsEnemyTurn())
            {
                // If the enemy is already dead, skip its turn
                if (!enemy.IsAlive)
                {
                    //LogToFile($"Skipping dead enemy at {enemy.GridPosition}.");
                    gameManager.ScheduleDelayedAction(0.5f, () => ProcessNextEnemy(index + 1));
                    return;
                }

                enemy.TakeTurn(() =>
                {
                    //LogToFile($"Enemy at {enemy.GridPosition} finished move. Processing next enemy...");
                    gameManager.ScheduleDelayedAction(0.5f, () => ProcessNextEnemy(index + 1));
                });
            }
            else
            {
                LogToFile($"ERROR: Tried to process enemy {index} outside EnemyTurn!");
            }
        }

        private void EndEnemyTurn()
        {
            //LogToFile("Enemy turn ended. Switching back to Player Turn.");
            CurrentTurn = TurnState.PlayerTurn;
            StartPlayerTurn();
        }

        // ----------------------------  LOGGING FOR DEBUGGING ----------------------------

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