using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Game_Eng_1
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _jogarButtonTexture;
        private Texture2D _creditosButtonTexture;
        private Texture2D _voltarButtonTexture;
        private Rectangle _jogarButtonRect;
        private Rectangle _creditosButtonRect;
        private Rectangle _voltarButtonRect;

        private Texture2D _heroTexture;
        private Vector2 _heroPosition;
        private Vector2 _heroVelocity;
        private float _gravity = 0.5f;
        private float _jumpStrength = -10f;

        private int _currentFrame = 0;
        private int _frameWidth;
        private int _frameHeight;
        private int _totalFrames;
        private float _animationSpeed = 0.06f;
        private float _timer = 0f;

        private Texture2D _terrenoTexture;
        private List<Rectangle> _terrenoRectangles;

        private Texture2D _espinhoTexture;
        private List<Rectangle> _espinhosRectangles;

        private Texture2D _trofeuTexture;
        private List<Vector2> _trofeuPositions;

        private List<Vector2> _trofeuColetados;

        private bool _showCredits = false;
        private bool _showHero = false;
        private bool _gameOver = false;

        private Texture2D _backgroundTexture;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _jogarButtonTexture = Content.Load<Texture2D>("JOGAR");
            _creditosButtonTexture = Content.Load<Texture2D>("CREDITOS");
            _voltarButtonTexture = Content.Load<Texture2D>("VOLTAR");

            int buttonWidth = 100;
            int buttonHeight = 50;
            int screenWidth = GraphicsDevice.Viewport.Width;
            int screenHeight = GraphicsDevice.Viewport.Height;

            int buttonX = (screenWidth - buttonWidth) / 2;
            int buttonY = (screenHeight - buttonHeight) / 2;

            _jogarButtonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
            _creditosButtonRect = new Rectangle(buttonX, buttonY + buttonHeight + 20, buttonWidth, buttonHeight);
            _voltarButtonRect = new Rectangle(20, 20, 100, 50);

            _heroTexture = Content.Load<Texture2D>("HERO");
            _frameWidth = _heroTexture.Width / 12;
            _frameHeight = _heroTexture.Height;
            _totalFrames = 12;

            _terrenoTexture = Content.Load<Texture2D>("TERRENO");
            _terrenoRectangles = new List<Rectangle>();

            _terrenoRectangles.Add(new Rectangle(0, screenHeight - 200, 46, 46));
            _terrenoRectangles.Add(new Rectangle(46, screenHeight - 200, 46, 46));
            _terrenoRectangles.Add(new Rectangle(130, screenHeight - 280, 46, 46));
            _terrenoRectangles.Add(new Rectangle(176, screenHeight - 280, 46, 46));
            _terrenoRectangles.Add(new Rectangle(326, screenHeight - 280, 46, 46));
            _terrenoRectangles.Add(new Rectangle(372, screenHeight - 280, 46, 46));
            _terrenoRectangles.Add(new Rectangle(520, screenHeight - 200, 46, 46));
            _terrenoRectangles.Add(new Rectangle(566, screenHeight - 200, 46, 46));
            _terrenoRectangles.Add(new Rectangle(700, screenHeight - 270, 46, 46));
            _terrenoRectangles.Add(new Rectangle(746, screenHeight - 270, 46, 46));

            _espinhoTexture = Content.Load<Texture2D>("ESPINHO");
            _espinhosRectangles = new List<Rectangle>();

            int numEspinhos = 20;
            int espacoEntreEspinhos = 40;
            int tamanhoEspinho = 48;

            for (int i = 0; i < numEspinhos; i++)
            {
                int espinhoX = i * espacoEntreEspinhos;
                _espinhosRectangles.Add(new Rectangle(espinhoX, screenHeight - tamanhoEspinho, tamanhoEspinho, tamanhoEspinho));
            }

            _trofeuTexture = Content.Load<Texture2D>("TROFEU");
            _trofeuPositions = new List<Vector2>();
            _trofeuColetados = new List<Vector2>();

            InitializeTrophies();

            _backgroundTexture = Content.Load<Texture2D>("BG");
        }

        private void InitializeTrophies()
        {
            _trofeuPositions.Clear();
            _trofeuPositions.Add(new Vector2(150, 130));
            _trofeuPositions.Add(new Vector2(350, 130));
            _trofeuPositions.Add(new Vector2(550, 210));
            _trofeuPositions.Add(new Vector2(730, 140));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                Point mousePosition = new Point(mouseState.X, mouseState.Y);

                if (_jogarButtonRect.Contains(mousePosition))
                {
                    _showHero = true;
                    _showCredits = false;
                }
                else if (_creditosButtonRect.Contains(mousePosition))
                {
                    _showCredits = true;
                }
                else if (_voltarButtonRect.Contains(mousePosition))
                {
                    if (_showHero)
                    {
                        _showHero = false;
                        ResetGame();
                    }
                    else if (_showCredits)
                    {
                        _showCredits = false;
                    }
                }
            }

            KeyboardState keyboardState = Keyboard.GetState();
            if (_showHero)
            {
                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    _heroVelocity.X = -3;
                }
                else if (keyboardState.IsKeyDown(Keys.Right))
                {
                    _heroVelocity.X = 3;
                }
                else
                {
                    _heroVelocity.X = 0;
                }

                if (keyboardState.IsKeyDown(Keys.Space) && (_heroPosition.Y == GraphicsDevice.Viewport.Height - _frameHeight || IsOnTerrain()))
                {
                    _heroVelocity.Y = _jumpStrength;
                }
            }

            _heroVelocity.Y += _gravity;

            HandleTerrainCollision();
            HandleSpikeCollision();
            HandleTrophyCollision();

            _heroPosition += _heroVelocity;

            if (_heroPosition.Y >= GraphicsDevice.Viewport.Height - _frameHeight)
            {
                _heroPosition.Y = GraphicsDevice.Viewport.Height - _frameHeight;
            }

            LimitHeroPosition();

            if (_showHero)
            {
                _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_timer > _animationSpeed)
                {
                    _currentFrame++;
                    if (_currentFrame >= _totalFrames)
                    {
                        _currentFrame = 0;
                    }
                    _timer = 0f;
                }
            }

            if (_trofeuColetados.Count == 4)
            {
                RestartGame();
            }

            base.Update(gameTime);
        }

        private void LimitHeroPosition()
        {
            _heroPosition.X = MathHelper.Clamp(_heroPosition.X, 0, GraphicsDevice.Viewport.Width - _frameWidth);
            _heroPosition.Y = MathHelper.Clamp(_heroPosition.Y, 0, GraphicsDevice.Viewport.Height - _frameHeight);
        }

        private bool IsOnTerrain()
        {
            foreach (Rectangle terrenoRect in _terrenoRectangles)
            {
                Rectangle heroRect = new Rectangle((int)_heroPosition.X, (int)_heroPosition.Y + _frameHeight, _frameWidth, 1);

                if (heroRect.Intersects(terrenoRect))
                {
                    return true;
                }
            }
            return false;
        }

        private void HandleTerrainCollision()
        {
            foreach (Rectangle terrenoRect in _terrenoRectangles)
            {
                Rectangle heroRect = new Rectangle((int)_heroPosition.X, (int)_heroPosition.Y, _frameWidth, _frameHeight);

                if (heroRect.Intersects(terrenoRect))
                {
                    _heroPosition.Y = terrenoRect.Y - _frameHeight;
                    _heroVelocity.Y = 0;
                }
            }
        }

        private void HandleSpikeCollision()
        {
            foreach (Rectangle spikeRect in _espinhosRectangles)
            {
                Rectangle heroRect = new Rectangle((int)_heroPosition.X, (int)_heroPosition.Y, _frameWidth, _frameHeight);

                if (heroRect.Intersects(spikeRect))
                {
                    GameOver();
                }
            }
        }

        private void HandleTrophyCollision()
        {
            Rectangle heroRect = new Rectangle((int)_heroPosition.X, (int)_heroPosition.Y, _frameWidth, _frameHeight);

            for (int i = 0; i < _trofeuPositions.Count; i++)
            {
                Vector2 trophyPosition = _trofeuPositions[i];
                Rectangle trophyRect = new Rectangle((int)trophyPosition.X, (int)trophyPosition.Y, _trofeuTexture.Width, _trofeuTexture.Height);

                if (heroRect.Intersects(trophyRect))
                {
                    _trofeuPositions.RemoveAt(i);
                    i--;

                    _trofeuColetados.Add(trophyPosition);
                }
            }
        }

        private void GameOver()
        {
            _gameOver = true;
            _showHero = false;
            ResetGame();
        }

        private void ResetGame()
        {
            _heroPosition = Vector2.Zero;
            _heroVelocity = Vector2.Zero;
            _gameOver = false;

            InitializeTrophies();

            _currentFrame = 0;
            _timer = 0f;

            _trofeuColetados.Clear();
        }

        private void RestartGame()
        {
            _showHero = false;
            _showCredits = false;
            _gameOver = false;
            _trofeuColetados.Clear();
            InitializeTrophies();
            _heroPosition = Vector2.Zero;
            _heroVelocity = Vector2.Zero;
            _currentFrame = 0;
            _timer = 0f;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            _spriteBatch.Draw(_backgroundTexture, Vector2.Zero, Color.White);

            if (_showHero)
            {
                foreach (var rect in _terrenoRectangles)
                {
                    _spriteBatch.Draw(_terrenoTexture, rect, Color.White);
                }

                foreach (var rect in _espinhosRectangles)
                {
                    _spriteBatch.Draw(_espinhoTexture, rect, Color.White);
                }

                foreach (Vector2 position in _trofeuPositions)
                {
                    _spriteBatch.Draw(_trofeuTexture, position, Color.White);
                }

                Rectangle sourceRectangle = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
                _spriteBatch.Draw(_heroTexture, _heroPosition, sourceRectangle, Color.White);

                _spriteBatch.Draw(_voltarButtonTexture, _voltarButtonRect, Color.White);
            }
            else if (_showCredits)
            {
                string creditsText = "Game criado por:\n Davy Woolley - 01529023" +
                    "\n Guilherme Leal- 01459560" +
                    " \n Vinicius Dantas - 01522137" +
                    " \n Arthur Felipe - 01505310 " +
                    "\n Leandro Manoel - 01519374 " +
                    "\n Diogo Bandeira - 01522133" +
                    "\n Rafael Lins - 01429504";

                Vector2 textSize = Content.Load<SpriteFont>("Font").MeasureString(creditsText);
                Vector2 textPosition = new Vector2((GraphicsDevice.Viewport.Width - textSize.X) / 2, (GraphicsDevice.Viewport.Height - textSize.Y) / 2);
                _spriteBatch.DrawString(Content.Load<SpriteFont>("Font"), creditsText, textPosition, Color.White);

                _spriteBatch.Draw(_voltarButtonTexture, _voltarButtonRect, Color.White);
            }
            else if (_gameOver)
            {
                string gameOverText = "Game Over";
                Vector2 textSize = Content.Load<SpriteFont>("Font").MeasureString(gameOverText);
                Vector2 textPosition = new Vector2((GraphicsDevice.Viewport.Width - textSize.X) / 2, (GraphicsDevice.Viewport.Height - textSize.Y) / 2);
                _spriteBatch.DrawString(Content.Load<SpriteFont>("Font"), gameOverText, textPosition, Color.Red);
            }
            else
            {
                _spriteBatch.Draw(_jogarButtonTexture, _jogarButtonRect, Color.White);
                _spriteBatch.Draw(_creditosButtonTexture, _creditosButtonRect, Color.White);
            }

            Vector2 trophyPosition = new Vector2(GraphicsDevice.Viewport.Width - _trofeuTexture.Width * 0.5f, 0);
            float spacing = 10;

            foreach (Vector2 trophy in _trofeuColetados)
            {
                _spriteBatch.Draw(_trofeuTexture, trophyPosition, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
                trophyPosition.X -= _trofeuTexture.Width * 0.5f + spacing;
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
