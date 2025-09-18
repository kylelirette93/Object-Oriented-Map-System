using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Object_Oriented_Map_System.Managers;
namespace Object_Oriented_Map_System
{
    public enum MainGameState
    {
        Title,
        Playing,
        GameOver
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameManager gameManager;

        private MainGameState currentGameState = MainGameState.Title;
        private SpriteFont uiFont;
        private MouseState previousMouseState;

        // Buttons
        private Rectangle startButton = new Rectangle(350, 200, 200, 50);
        private Rectangle quitButton = new Rectangle(350, 270, 200, 50);
        private Rectangle playAgainButton = new Rectangle(350, 200, 200, 50);
        private Rectangle quitToMenuButton = new Rectangle(350, 270, 200, 50);
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        public Game1()
        {
            AllocConsole();
            Console.WriteLine("Debug console initialized.");
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            gameManager = new GameManager(_graphics, Content);
            gameManager.player.OnPlayerDeath = () => currentGameState = MainGameState.GameOver;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            uiFont = Content.Load<SpriteFont>("DamageFont");
            gameManager.LoadContent();

            gameManager.OnFadeComplete = () =>
            {
                currentGameState = MainGameState.GameOver;
            };
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                keyboard.IsKeyDown(Keys.Escape))
                Exit();

            if (currentGameState == MainGameState.Title)
            {
                Point mousePos = new Point(mouse.X, mouse.Y);

                if (mouse.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                {
                    if (startButton.Contains(mousePos))
                    {
                        currentGameState = MainGameState.Playing;
                        gameManager = new GameManager(_graphics, Content);
                        gameManager.player.OnPlayerDeath = () =>
                        {
                            currentGameState = MainGameState.GameOver;
                        };
                        gameManager.OnFadeComplete = () =>
                        {
                            currentGameState = MainGameState.GameOver;
                        };
                        gameManager.LoadContent();
                    }
                    else if (quitButton.Contains(mousePos))
                    {
                        Exit();
                    }
                }
            }
            else if (currentGameState == MainGameState.Playing)
            {
                gameManager.Update(gameTime);
            }
            else if (currentGameState == MainGameState.GameOver)
            {
                Point mousePos = new Point(mouse.X, mouse.Y);

                if (mouse.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                {
                    if (playAgainButton.Contains(mousePos))
                    {
                        RestartGame();
                    }
                    else if (quitToMenuButton.Contains(mousePos))
                    {
                        currentGameState = MainGameState.Title;
                    }
                }
            }

            previousMouseState = mouse;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Color backgroundColor = currentGameState == MainGameState.Playing ? Color.CornflowerBlue : Color.Black;
            GraphicsDevice.Clear(backgroundColor);

            _spriteBatch.Begin();

            if (currentGameState == MainGameState.Title)
            {
                _spriteBatch.DrawString(uiFont, "My 2D RPG", new Vector2(360, 100), Color.White);
                _spriteBatch.Draw(CreateRectangleTexture(startButton.Width, startButton.Height, Color.Gray), startButton, Color.White);
                _spriteBatch.DrawString(uiFont, "Start", new Vector2(410, 215), Color.Black);
                _spriteBatch.Draw(CreateRectangleTexture(quitButton.Width, quitButton.Height, Color.Gray), quitButton, Color.White);
                _spriteBatch.DrawString(uiFont, "Quit", new Vector2(420, 285), Color.Black);
            }
            else if (currentGameState == MainGameState.GameOver)
            {
                _spriteBatch.DrawString(uiFont, "Game Over", new Vector2(360, 100), Color.Red);
                _spriteBatch.Draw(CreateRectangleTexture(playAgainButton.Width, playAgainButton.Height, Color.Gray), playAgainButton, Color.White);
                _spriteBatch.DrawString(uiFont, "Play Again", new Vector2(375, 215), Color.Black);
                _spriteBatch.Draw(CreateRectangleTexture(quitToMenuButton.Width, quitToMenuButton.Height, Color.Gray), quitToMenuButton, Color.White);
                _spriteBatch.DrawString(uiFont, "Quit to Menu", new Vector2(365, 285), Color.Black);
            }

            _spriteBatch.End();

            if (currentGameState == MainGameState.Playing)
            {
                gameManager.Draw(_spriteBatch);
            }

            base.Draw(gameTime);
        }

        private Texture2D CreateRectangleTexture(int width, int height, Color color)
        {
            Texture2D rect = new Texture2D(GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i) data[i] = color;
            rect.SetData(data);
            return rect;
        }

        private void RestartGame()
        {
            currentGameState = MainGameState.Playing;
            gameManager = new GameManager(_graphics, Content);
            gameManager.player.OnPlayerDeath = () =>
            {
                currentGameState = MainGameState.GameOver;
            };
            gameManager.OnFadeComplete = () =>
            {
                currentGameState = MainGameState.GameOver;
            };
            gameManager.LoadContent();
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