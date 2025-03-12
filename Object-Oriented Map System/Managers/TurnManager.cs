using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Object_Oriented_Map_System.Managers;
using Object_Oriented_Map_System.Entities;

namespace Object_Oriented_Map_System.Managers
{
    public enum TurnPhase { PlayerTurn, EnemyTurn }

    public class TurnManager
    {
        private TurnPhase currentPhase;
        private GameManager gameManager;
        private int enemyIndex = 0;  // Tracks which enemy is currently acting

        public TurnManager(GameManager manager)
        {
            gameManager = manager;
            currentPhase = TurnPhase.PlayerTurn;
        }

        public void StartPlayerTurn()
        {
            currentPhase = TurnPhase.PlayerTurn;
            enemyIndex = 0; // Reset enemy tracking
        }

        public void StartEnemyTurn()
        {
            currentPhase = TurnPhase.EnemyTurn;
            enemyIndex = 0;
        }

        public void Update(GameTime gameTime)
        {
            if (currentPhase == TurnPhase.PlayerTurn)
            {
                gameManager.HandlePlayerTurn(Keyboard.GetState());
            }
            else if (currentPhase == TurnPhase.EnemyTurn)
            {
                if (gameManager.Enemies.Count > 0)
                {
                    HandleEnemyTurn(gameTime);
                }
                else
                {
                    StartPlayerTurn(); // No enemies left, back to player
                }
            }
        }

        public bool IsPlayerTurn()
        {
            return currentPhase == TurnPhase.PlayerTurn;
        }

        private void HandleEnemyTurn(GameTime gameTime)
        {
            if (enemyIndex < gameManager.Enemies.Count)
            {
                Enemy currentEnemy = gameManager.Enemies[enemyIndex];

                if (!currentEnemy.IsStunned)
                {
                    currentEnemy.TakeTurn();
                }
                else
                {
                    currentEnemy.RecoverFromStun();
                }

                enemyIndex++; // Move to next enemy

                if (enemyIndex >= gameManager.Enemies.Count)
                {
                    StartPlayerTurn(); // After last enemy, switch back to player
                }
            }
        }
    }
}