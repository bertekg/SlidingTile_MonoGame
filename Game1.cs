using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using static SlidingTile_MonoGame.Game1;
using System.Xml.Serialization;

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

        private bool _playerMovePossible;
        private bool _playerMoveInital;
        private float _playerMoveSpeed;
        private double _timeMoveMax, _timeMoveCurrent;
        private float _stepDistans;
        private Vector2 _moveDirection, _playerPosInit;

        private Texture2D _floorTileTexture2D;

        private SpriteFont _digitFloor;

        List<Cell> _cells;
        Vector2 _levelStart;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Window.Title = "Sliding Tile - MonoGame (0.1 - 2022.10.12)";

            _levelStart = new Vector2(5, 6);

            _playerPosition = new Vector2(100 * _levelStart.X, 100 * _levelStart.Y);

            _playerSpeed = 250.0d;

            _playerMovePossible = true;
            _playerMoveInital = false;

            _timeMoveMax = 0.5d;
            _timeMoveCurrent = 0.0d;
            _stepDistans = 100.0f;

            _moveDirection = new Vector2();

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            _cells = new List<Cell>();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("LevelBasic/0_Basic_01.xml");
            string xmlString = xmlDocument.OuterXml;
            using (StringReader read = new StringReader(xmlString))
            {
                Type outType = typeof(List<Cell>);

                XmlSerializer serializer = new XmlSerializer(outType);
                using (XmlReader reader = new XmlTextReader(read))
                {
                    _cells = (List<Cell>)serializer.Deserialize(reader);
                    reader.Close();
                }
                read.Close();
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _playerTexture2D = Content.Load<Texture2D>("sprites/player");
            _floorTileTexture2D = Content.Load<Texture2D>("sprites/floorTile");

            _digitFloor = Content.Load<SpriteFont>("fonts/digitFloor");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _currChangeValue = (float)(_playerSpeed * gameTime.ElapsedGameTime.TotalSeconds);

            if (Keyboard.GetState().IsKeyDown(Keys.Up) && _playerMovePossible)
            {
                _moveDirection = new Vector2(0.0f, -1.0f);
                _playerMovePossible = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down) && _playerMovePossible)
            {
                _moveDirection = new Vector2(0.0f, 1.0f);
                _playerMovePossible = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left) && _playerMovePossible)
            {
                _moveDirection = new Vector2(-1.0f, 0.0f);
                _playerMovePossible = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right) && _playerMovePossible)
            {
                _moveDirection = new Vector2(1.0f, 0.0f);
                _playerMovePossible = false;
            }
            MovePlayer(gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            for (int i = 0; i < _cells.Count; i++)
            {
                Vector2 finalPosition = new Vector2(100*((int)_levelStart.X + _cells[i].PosX), 100 * ((int)_levelStart.Y - _cells[i].PosY));
                _spriteBatch.Draw(_floorTileTexture2D, finalPosition, Color.White);
                string textInside = string.Empty;
                Color colorText = new Color();
                Vector2 vectorTextOffset = new Vector2();
                switch (_cells[i].Type)
                {
                    case CellType.None:
                        break;
                    case CellType.Finish:
                        textInside = "F";
                        colorText = Color.Red;
                        vectorTextOffset = new Vector2(35, 15);
                        break;
                    case CellType.Normal:
                        if (_cells[i].PosX != 0 || _cells[i].PosY != 0)
                        {
                            textInside = _cells[i].Number.ToString();
                            colorText = Color.Black;
                            vectorTextOffset = new Vector2(35, 15);
                        }
                        else
                        {
                            textInside = "S" + _cells[i].Number.ToString();
                            colorText = Color.Green;
                            vectorTextOffset = new Vector2(15, 15);
                        }
                        break;
                    case CellType.Ice:
                        break;
                    default:
                        break;
                }
                _spriteBatch.DrawString(_digitFloor, textInside, finalPosition + vectorTextOffset, colorText);
            }

            _spriteBatch.Draw(_playerTexture2D, _playerPosition, Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
        private void MovePlayer(double totalSecounds)
        {
            if(_playerMovePossible == false & _playerMoveInital == false)
            {
                _playerMoveSpeed = _stepDistans / (float)_timeMoveMax;
                _timeMoveCurrent = 0.0d;
                _playerMoveInital = true;
                _playerPosInit = _playerPosition; 
            }
            _currChangeValue = (float)(_playerMoveSpeed * totalSecounds);
            if (_playerMovePossible == false & _playerMoveInital == true)
            {
                if (_timeMoveCurrent <= _timeMoveMax)
                {
                    _playerPosition += new Vector2(_currChangeValue * _moveDirection.X, _currChangeValue * _moveDirection.Y);
                    _timeMoveCurrent += totalSecounds;
                }
                else
                {
                    _playerPosition = new Vector2(_playerPosInit.X + (_stepDistans * _moveDirection.X), _playerPosInit.Y + (_stepDistans * _moveDirection.Y));
                    _playerMoveInital = false;
                    _playerMovePossible = true;
                }
            }
        }
        public enum CellType { None, Finish, Normal, Ice };
        public class Cell
        {
            public int PosX { get; set; }
            public int PosY { get; set; }
            public CellType Type { get; set; }
            public int Number { get; set; }
        }
    }
}