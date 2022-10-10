using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlidingTile_MonoGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _playerTexture2D;

        private Vector2 _playerPosition;

        private double _playerSpeed;
        private float _currChangeValue;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _playerPosition = new Vector2(540, 260);

            _playerSpeed = 250.0d;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _playerTexture2D = Content.Load<Texture2D>("sprites/player");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _currChangeValue = (float)(_playerSpeed * gameTime.ElapsedGameTime.TotalSeconds);

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                _playerPosition.Y -= _currChangeValue;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                _playerPosition.Y += _currChangeValue;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                _playerPosition.X -= _currChangeValue;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                _playerPosition.X += _currChangeValue;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            _spriteBatch.Draw(_playerTexture2D, _playerPosition, Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}