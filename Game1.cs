using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
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
        private SpriteFont _debugGame;

        List<Cell> _floorTiles;
        Vector2 _levelStart;
        Cell _existingCell;

        List<MoveCommand> _moveCommands;
        int _moveCommandsIndex;
        Vector2 debugMoveCountPosition;
        Vector2 debugMoveListPosition;
        bool _playerUndoMove;
        bool _playerRedoMove;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Window.Title = "Sliding Tile - MonoGame (0.2 - 2022.10.17)";

            _floorTiles = new List<Cell>();
            _existingCell = new Cell();
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
            _existingCell = _floorTiles.Find(pos => pos.PosX == 0 && pos.PosY == 0);
            _existingCell.Number -= 1;
            int indexTile = _floorTiles.FindIndex(item => item.PosX == _existingCell.PosX && item.PosY == _existingCell.PosY);
            _floorTiles[indexTile] = _existingCell;

            _levelStart = GetLevelStart();

            _playerPosition = new Vector2(100 * _levelStart.X, 100 * _levelStart.Y);
            _playerVirtualPoint = new Point(0, 0);
            _playerVirtualPointDestination = new Point(0, 0);

            _playerPerformMove = false;
            _playerMoveInital = false;

            _timeMoveMax = 0.3d;
            _timeMoveCurrent = 0.0d;
            _stepDistans = 100.0f;

            _moveVerse = new Vector2();

            _moveCommands = new List<MoveCommand>();
            _moveCommandsIndex = 0;
            debugMoveCountPosition = new Vector2(20, 680);
            debugMoveListPosition = new Vector2(780, 20);
            _playerUndoMove = false;
            _playerRedoMove = false;

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
            _debugGame = Content.Load<SpriteFont>("fonts/debugGame");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Up) && _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
            {
                _moveVerse = new Vector2(0.0f, -1.0f);
                _playerPerformMove = true;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down) && _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
            {
                _moveVerse = new Vector2(0.0f, 1.0f);
                _playerPerformMove = true;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left) && _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
            {
                _moveVerse = new Vector2(-1.0f, 0.0f);
                _playerPerformMove = true;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right) && _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
            {
                _moveVerse = new Vector2(1.0f, 0.0f);
                _playerPerformMove = true;
            }
            if(Keyboard.GetState().IsKeyDown(Keys.U) && _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
            {
                _playerUndoMove = true;
                _playerPerformMove = true;
            }
            if(Keyboard.GetState().IsKeyDown(Keys.R) && _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
            {
                _playerRedoMove = true;
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
                if (_floorTiles[i].Type == CellType.Normal && _floorTiles[i].Number == 0) continue;
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

            _spriteBatch.DrawString(_debugGame, "Player move commands: counts " + _moveCommands.Count.ToString() + ", index " + _moveCommandsIndex.ToString(), debugMoveCountPosition, Color.White);
            for (int i = 0; i < _moveCommands.Count; i++)
            {
                Vector2 verticalOffset = new Vector2(0, 28 * i);
                Point start = _moveCommands[i].GetStartPoint();
                Point end = _moveCommands[i].GetEndPoint();
                Color currentIndex = Color.White;
                if (i == _moveCommandsIndex - 1)
                {
                    currentIndex = Color.Green;
                }
                _spriteBatch.DrawString(_debugGame, "[" + i.ToString() + "] Start [" + start.X.ToString() + "," + start.Y.ToString() + "], End [" + end.X.ToString() + 
                    "," + end.Y.ToString() + "], Cells mod count: " + _moveCommands[i].GetModifiedCellsBefore().Count.ToString(), debugMoveListPosition + verticalOffset, currentIndex);
            }

            _spriteBatch.Draw(_playerTexture2D, _playerPosition, Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
        private void MovePlayer(double totalSecounds)
        {
            if(_playerPerformMove == true && _playerMoveInital == false && _playerUndoMove == false && _playerRedoMove == false)
            {
                _playerVirtualPointDestination = _playerVirtualPoint + new Point((int)_moveVerse.X, -(int)_moveVerse.Y);
                _existingCell = _floorTiles.Find(pos => pos.PosX == _playerVirtualPointDestination.X && pos.PosY == _playerVirtualPointDestination.Y);
                if (_existingCell != null)
                {
                    if (_existingCell.Number > 0 || _existingCell.Type == CellType.Finish)
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
                else
                {
                    _playerPerformMove = false;
                }
            }
            if(_playerPerformMove == true && _playerMoveInital == false && _playerUndoMove == true && _playerRedoMove == false)
            {
                if (_moveCommandsIndex > 0)
                {
                    MoveCommand moveCommand = _moveCommands[_moveCommandsIndex - 1];
                    _playerVirtualPointDestination = moveCommand.GetStartPoint();
                    _playerMoveSpeed = _stepDistans / (float)_timeMoveMax;
                    _timeMoveCurrent = 0.0d;
                    _playerMoveInital = true;
                    _playerPosInit = _playerPosition;
                    _moveVerse = new Vector2(moveCommand.GetStartPoint().X - moveCommand.GetEndPoint().X, -(moveCommand.GetStartPoint().Y - moveCommand.GetEndPoint().Y));

                    List<Cell> cells = _moveCommands[_moveCommandsIndex - 1].GetModifiedCellsBefore();
                    foreach (Cell cell in cells)
                    {
                        int indexTile = _floorTiles.FindIndex(item => item.PosX == cell.PosX && item.PosY == cell.PosY);
                        _floorTiles[indexTile] = cell;
                    }
                }
                else
                {
                    _playerPerformMove = false;
                    _playerUndoMove = false;
                }
            }
            if (_playerPerformMove == true && _playerMoveInital == false && _playerUndoMove == false && _playerRedoMove == true)
            {
                if (_moveCommandsIndex < _moveCommands.Count)
                {
                    MoveCommand moveCommand = _moveCommands[_moveCommandsIndex];
                    _playerVirtualPointDestination = moveCommand.GetEndPoint();
                    _playerMoveSpeed = _stepDistans / (float)_timeMoveMax;
                    _timeMoveCurrent = 0.0d;
                    _playerMoveInital = true;
                    _playerPosInit = _playerPosition;
                    _moveVerse = new Vector2(moveCommand.GetEndPoint().X - moveCommand.GetStartPoint().X, -(moveCommand.GetEndPoint().Y - moveCommand.GetStartPoint().Y));
                }
                else
                {
                    _playerPerformMove = false;
                    _playerRedoMove = false;
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

                    if (_playerUndoMove == false && _playerRedoMove == false)
                    {
                        if (_moveCommandsIndex < _moveCommands.Count)
                        {
                            int howManyDelete = _moveCommands.Count - _moveCommandsIndex;
                            for (int i = 0; i < howManyDelete; i++)
                            {
                                _moveCommands.RemoveAt(_moveCommands.Count - 1);
                            }
                        }
                        List<Cell> modCellsBefore = new List<Cell>();
                        List<Cell> modCellsAfter = new List<Cell>();
                        if (_existingCell.Type == CellType.Normal)
                        {
                            int indexTile = _floorTiles.FindIndex(item => item.PosX == _existingCell.PosX && item.PosY == _existingCell.PosY);
                            modCellsBefore.Add(new Cell() { Number = _existingCell.Number, PosX = _existingCell.PosX, PosY = _existingCell.PosY, Type = _existingCell.Type});
                            _existingCell.Number -= 1;
                            _floorTiles[indexTile].Number = _existingCell.Number;
                            modCellsAfter.Add(new Cell() { Number = _existingCell.Number, PosX = _existingCell.PosX, PosY = _existingCell.PosY, Type = _existingCell.Type });
                        }
                        _moveCommands.Add(new MoveCommand(_playerVirtualPoint, _playerVirtualPointDestination, modCellsBefore, modCellsAfter));
                        _moveCommandsIndex = _moveCommands.Count;
                        
                    }
                    if (_playerUndoMove == true)
                    {
                        _moveCommandsIndex -= 1;
                        _playerUndoMove = false;
                    }
                    if (_playerRedoMove == true)
                    {   
                        List<Cell> cells = _moveCommands[_moveCommandsIndex].GetModifiedCellsAfter();
                        foreach (Cell cell in cells)
                        {
                            int indexTile = _floorTiles.FindIndex(item => item.PosX == cell.PosX && item.PosY == cell.PosY);
                            _floorTiles[indexTile] = cell;
                        }
                        _moveCommandsIndex += 1;
                        _playerRedoMove = false;
                    }
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