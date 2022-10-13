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
using System.Linq;

namespace SlidingTile_MonoGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _playerTexture2D;

        private Vector2 _playerPosition;
        private Point _playerVirtualPoint;
        private Point _playerVirtualPointDestination;

        private float _currChangeValue;

        private bool _playerPerformMove;
        private bool _playerMoveInital;
        private float _playerMoveSpeed;
        private double _timeMoveMax, _timeMoveCurrent;
        private float _stepDistans;
        private Vector2 _moveVerse, _playerPosInit;

        private Texture2D _floorTileTexture2D;

        private SpriteFont _digitFloor;

        List<Cell> _floorTiles;
        Vector2 _levelStart;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Window.Title = "Sliding Tile - MonoGame (0.1.1 - 2022.10.13)";

            _floorTiles = new List<Cell>();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("LevelBasic/0_Basic_01.xml");
            string xmlString = xmlDocument.OuterXml;
            using (StringReader read = new StringReader(xmlString))
            {
                Type outType = typeof(List<Cell>);

                XmlSerializer serializer = new XmlSerializer(outType);
                using (XmlReader reader = new XmlTextReader(read))
                {
                    _floorTiles = (List<Cell>)serializer.Deserialize(reader);
                    reader.Close();
                }
                read.Close();
            }

            _levelStart = GetLevelStart();

            _playerPosition = new Vector2(100 * _levelStart.X, 100 * _levelStart.Y);
            _playerVirtualPoint = new Point(0, 0);
            _playerVirtualPointDestination = new Point(0, 0);

            _playerPerformMove = false;
            _playerMoveInital = false;

            _timeMoveMax = 0.5d;
            _timeMoveCurrent = 0.0d;
            _stepDistans = 100.0f;

            _moveVerse = new Vector2();

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

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

            if (Keyboard.GetState().IsKeyDown(Keys.Up) && _playerPerformMove == false)
            {
                _moveVerse = new Vector2(0.0f, -1.0f);
                _playerPerformMove = true;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down) && _playerPerformMove == false)
            {
                _moveVerse = new Vector2(0.0f, 1.0f);
                _playerPerformMove = true;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left) && _playerPerformMove == false)
            {
                _moveVerse = new Vector2(-1.0f, 0.0f);
                _playerPerformMove = true;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right) && _playerPerformMove == false)
            {
                _moveVerse = new Vector2(1.0f, 0.0f);
                _playerPerformMove = true;
            }
            MovePlayer(gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            for (int i = 0; i < _floorTiles.Count; i++)
            {
                Vector2 finalPosition = new Vector2(100*((int)_levelStart.X + _floorTiles[i].PosX), 100 * ((int)_levelStart.Y - _floorTiles[i].PosY));
                _spriteBatch.Draw(_floorTileTexture2D, finalPosition, Color.White);
                string textInside = string.Empty;
                Color colorText = new Color();
                Vector2 vectorTextOffset = new Vector2();
                switch (_floorTiles[i].Type)
                {
                    case CellType.None:
                        break;
                    case CellType.Finish:
                        textInside = "F";
                        colorText = Color.Red;
                        vectorTextOffset = new Vector2(35, 15);
                        break;
                    case CellType.Normal:
                        if (_floorTiles[i].PosX != 0 || _floorTiles[i].PosY != 0)
                        {
                            textInside = _floorTiles[i].Number.ToString();
                            colorText = Color.Black;
                            vectorTextOffset = new Vector2(35, 15);
                        }
                        else
                        {
                            textInside = "S" + _floorTiles[i].Number.ToString();
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
            if(_playerPerformMove == true & _playerMoveInital == false)
            {
                _playerVirtualPointDestination = _playerVirtualPoint + new Point((int)_moveVerse.X, -(int)_moveVerse.Y);
                bool cellFloorExist = _floorTiles.Any(pos => pos.PosX == _playerVirtualPointDestination.X && pos.PosY == _playerVirtualPointDestination.Y);
                if (cellFloorExist)
                {
                    _playerMoveSpeed = _stepDistans / (float)_timeMoveMax;
                    _timeMoveCurrent = 0.0d;
                    _playerMoveInital = true;
                    _playerPosInit = _playerPosition;
                }
                else
                {
                    _playerPerformMove = false;
                }
            }
            _currChangeValue = (float)(_playerMoveSpeed * totalSecounds);
            if (_playerPerformMove == true & _playerMoveInital == true)
            {
                if (_timeMoveCurrent <= _timeMoveMax)
                {
                    _playerPosition += new Vector2(_currChangeValue * _moveVerse.X, _currChangeValue * _moveVerse.Y);
                    _timeMoveCurrent += totalSecounds;
                }
                else
                {
                    _playerPosition = new Vector2(_playerPosInit.X + (_stepDistans * _moveVerse.X), _playerPosInit.Y + (_stepDistans * _moveVerse.Y));
                    _playerMoveInital = false;
                    _playerPerformMove = false;
                    _playerVirtualPoint = _playerVirtualPointDestination;
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
        private Vector2 GetLevelStart()
        {
            int offestX = -_floorTiles.Min(posX => posX.PosX);
            int offsetY = _floorTiles.Max(posY => posY.PosY);
            return new Vector2(offestX, offsetY);
        }
    }
}