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
        public static int ScreenWidth = 1900;
        public static int ScreenHeight = 980;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _playerTexture2D;

        private Vector2 _playerPosition;
        private Point _playerVirtualPoint;
        private Point _playerVirtualPointDestination;

        private float _currChangeValue;

        private bool _isDuringGame;
        private bool _playerPerformMove;
        private bool _playerMoveInital;
        private float _playerMoveSpeed;
        private double _timeMoveMax, _timeMoveCurrent;
        private float _stepDistans;
        private Vector2 _moveVerse, _playerPosInit;

        private Texture2D _floorTileTexture2D;

        private SpriteFont _digitFloor;
        private SpriteFont _debugGame;
        private SpriteFont _gameEnd;

        List<FloorTile> _floorTiles;
        Vector2 _levelStart;
        FloorTile _currentFloorTile;

        List<MoveCommand> _moveCommands;
        int _moveCommandsIndex;
        bool _playerUndoMove;
        bool _playerRedoMove;

        bool _gameFinishSuccesfull;
        private Texture2D _endGamePanelTexture2D;

        private Vector2 _endGamePanelPosition, _endGamePanelGameOverLoc;
        private Vector2 _endGamePanelFinishStatusWinLoc, _endGamePanelFinishStatusLoseLoc, _endGamePanelSpaceToStartAgainLoc;

        private Vector2 _debugMoveCountPosition, _debugMoveListPosition;

        private string _endGamePanelFinishGameOver, _endGamePanelFinishStatusLose;
        private string _endGamePanelFinishStatusWin, _endGamePanelSpaceToStartAgainText;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        protected override void Initialize()
        {
            Window.Title = "Sliding Tile - MonoGame (0.3.0 - 2022.11.04)";

            StartNewGame();

            _timeMoveMax = 0.3d;
            _timeMoveCurrent = 0.0d;
            _stepDistans = 100.0f;

            _endGamePanelFinishGameOver = "Game Over!!!";
            _endGamePanelFinishStatusWin = "You WIN :)";
            _endGamePanelFinishStatusLose = "You LOSE :(";
            _endGamePanelSpaceToStartAgainText = "[Space, START] - Restart game";

            _moveVerse = new Vector2();

            _graphics.PreferredBackBufferWidth = ScreenWidth;
            _graphics.PreferredBackBufferHeight = ScreenHeight;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();

            base.Initialize();
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _playerTexture2D = Content.Load<Texture2D>("sprites/player");
            _floorTileTexture2D = Content.Load<Texture2D>("sprites/floorTile");
            _endGamePanelTexture2D = Content.Load<Texture2D>("sprites/endGamePanel");

            _endGamePanelPosition = new Vector2(
                (_graphics.PreferredBackBufferWidth - _endGamePanelTexture2D.Width) / 2,
                (_graphics.PreferredBackBufferHeight - _endGamePanelTexture2D.Height) / 2);
            
            
            _digitFloor = Content.Load<SpriteFont>("fonts/digitFloor");
            _debugGame = Content.Load<SpriteFont>("fonts/debugGame");
            _gameEnd = Content.Load<SpriteFont>("fonts/gameEnd");

            _endGamePanelGameOverLoc.X = _endGamePanelPosition.X + (_endGamePanelTexture2D.Width / 2)
                - (_gameEnd.MeasureString(_endGamePanelFinishGameOver).X / 2);
            _endGamePanelGameOverLoc.Y = _endGamePanelPosition.Y + 20;

            _endGamePanelFinishStatusWinLoc.X = _endGamePanelPosition.X + (_endGamePanelTexture2D.Width / 2) 
                - (_gameEnd.MeasureString(_endGamePanelFinishStatusWin).X / 2);
            _endGamePanelFinishStatusWinLoc.Y = _endGamePanelPosition.Y + 120;

            _endGamePanelFinishStatusLoseLoc.X = _endGamePanelPosition.X + (_endGamePanelTexture2D.Width / 2)
               - (_gameEnd.MeasureString(_endGamePanelFinishStatusLose).X / 2);
            _endGamePanelFinishStatusLoseLoc.Y = _endGamePanelPosition.Y + 120;

            _endGamePanelSpaceToStartAgainLoc.X = _endGamePanelPosition.X + (_endGamePanelTexture2D.Width / 2)
                - (_debugGame.MeasureString(_endGamePanelSpaceToStartAgainText).X / 2);
            _endGamePanelSpaceToStartAgainLoc.Y = _endGamePanelPosition.Y + 220;

            _debugMoveCountPosition = new Vector2(20, ScreenHeight - 
                _debugGame.MeasureString("Player move commands: counts").Y - 20);
            _debugMoveListPosition = new Vector2(ScreenWidth - 
                _debugGame.MeasureString("[99] Start [00,00], End [00,00], Floor tile mod before count: 00").X - 20, 20);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            if (keyboardState.IsKeyDown(Keys.Escape) || gamePadState.IsButtonDown(Buttons.Back))
                Exit();

            if (_isDuringGame)
            {
                if ((keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W) || gamePadState.IsButtonDown(Buttons.DPadUp))
                    && _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
                {
                    _moveVerse = new Vector2(0.0f, -1.0f);
                    _playerPerformMove = true;
                }

                if ((keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S) || gamePadState.IsButtonDown(Buttons.DPadDown))
                    && _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
                {
                    _moveVerse = new Vector2(0.0f, 1.0f);
                    _playerPerformMove = true;
                }

                if ((keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A) || gamePadState.IsButtonDown(Buttons.DPadLeft))
                    && _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
                {
                    _moveVerse = new Vector2(-1.0f, 0.0f);
                    _playerPerformMove = true;
                }

                if ((keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D) || gamePadState.IsButtonDown(Buttons.DPadRight))
                    && _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
                {
                    _moveVerse = new Vector2(1.0f, 0.0f);
                    _playerPerformMove = true;
                }
                if ((keyboardState.IsKeyDown(Keys.U) || gamePadState.IsButtonDown(Buttons.LeftShoulder))
                    && _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
                {
                    _playerUndoMove = true;
                    _playerPerformMove = true;
                }
                if ((keyboardState.IsKeyDown(Keys.R) || gamePadState.IsButtonDown(Buttons.RightShoulder))
                    && _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
                {
                    _playerRedoMove = true;
                    _playerPerformMove = true;
                }
                MovePlayer(gameTime.ElapsedGameTime.TotalSeconds);
            }
            else
            {
                if (keyboardState.IsKeyDown(Keys.Space) || gamePadState.IsButtonDown(Buttons.Start))
                {
                    StartNewGame();
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            for (int i = 0; i < _floorTiles.Count; i++)
            {
                if (_floorTiles[i].Type == FloorTileType.Normal && _floorTiles[i].Number == 0) continue;
                Vector2 finalPosition = new Vector2(100*((int)_levelStart.X + _floorTiles[i].PosX), 100 * ((int)_levelStart.Y - _floorTiles[i].PosY));
                _spriteBatch.Draw(_floorTileTexture2D, finalPosition, Color.White);
                string textInside = string.Empty;
                Color colorText = new Color();
                Vector2 vectorTextOffset = new Vector2();
                switch (_floorTiles[i].Type)
                {
                    case FloorTileType.None:
                        break;
                    case FloorTileType.Finish:
                        textInside = "F";
                        colorText = Color.Red;
                        vectorTextOffset = new Vector2(35, 15);
                        break;
                    case FloorTileType.Normal:
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
                    case FloorTileType.Ice:
                        break;
                    default:
                        break;
                }
                _spriteBatch.DrawString(_digitFloor, textInside, finalPosition + vectorTextOffset, colorText);
            }

            _spriteBatch.DrawString(_debugGame, "Player move commands: counts " + _moveCommands.Count.ToString() + ", index " + _moveCommandsIndex.ToString(), _debugMoveCountPosition, Color.White);
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
                    "," + end.Y.ToString() + "], Floor tile mod before count: " + _moveCommands[i].GetModifiedFloorTileBefore().Count.ToString(), _debugMoveListPosition + verticalOffset, currentIndex);
            }

            _spriteBatch.Draw(_playerTexture2D, _playerPosition, Color.White);

            if (_isDuringGame == false)
            {
                _spriteBatch.Draw(_endGamePanelTexture2D, _endGamePanelPosition, Color.White);
                _spriteBatch.DrawString(_gameEnd, _endGamePanelFinishGameOver, _endGamePanelGameOverLoc, Color.White);
                if (_gameFinishSuccesfull)
                {
                    _spriteBatch.DrawString(_gameEnd, _endGamePanelFinishStatusWin, _endGamePanelFinishStatusWinLoc, Color.GreenYellow);
                }
                else
                {
                    _spriteBatch.DrawString(_gameEnd, _endGamePanelFinishStatusLose, _endGamePanelFinishStatusLoseLoc, Color.Red);
                }
                _spriteBatch.DrawString(_debugGame, _endGamePanelSpaceToStartAgainText, _endGamePanelSpaceToStartAgainLoc, Color.White);

            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
        private void MovePlayer(double totalSecounds)
        {
            if(_playerPerformMove == true && _playerMoveInital == false && _playerUndoMove == false && _playerRedoMove == false)
            {
                _playerVirtualPointDestination = _playerVirtualPoint + new Point((int)_moveVerse.X, -(int)_moveVerse.Y);
                _currentFloorTile = _floorTiles.Find(pos => pos.PosX == _playerVirtualPointDestination.X && pos.PosY == _playerVirtualPointDestination.Y);
                if (_currentFloorTile != null)
                {
                    if (_currentFloorTile.Number > 0 || _currentFloorTile.Type == FloorTileType.Finish)
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

                    List<FloorTile> floorTiles = _moveCommands[_moveCommandsIndex - 1].GetModifiedFloorTileBefore();
                    foreach (FloorTile floorTile in floorTiles)
                    {
                        int indexTile = _floorTiles.FindIndex(item => item.PosX == floorTile.PosX && item.PosY == floorTile.PosY);
                        _floorTiles[indexTile] = floorTile;
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
                        List<FloorTile> modFloorTilesBefore = new List<FloorTile>();
                        List<FloorTile> modFloorTilesAfter = new List<FloorTile>();
                        if (_currentFloorTile.Type == FloorTileType.Normal)
                        {
                            int indexTile = _floorTiles.FindIndex(item => item.PosX == _currentFloorTile.PosX && item.PosY == _currentFloorTile.PosY);
                            modFloorTilesBefore.Add(new FloorTile() { Number = _currentFloorTile.Number, PosX = _currentFloorTile.PosX, PosY = _currentFloorTile.PosY, Type = _currentFloorTile.Type});
                            _currentFloorTile.Number -= 1;
                            _floorTiles[indexTile].Number = _currentFloorTile.Number;
                            modFloorTilesAfter.Add(new FloorTile() { Number = _currentFloorTile.Number, PosX = _currentFloorTile.PosX, PosY = _currentFloorTile.PosY, Type = _currentFloorTile.Type });
                        }
                        else if(_currentFloorTile.Type == FloorTileType.Finish)
                        {
                            bool answere = true;
                            foreach (FloorTile tile in _floorTiles)
                            {
                                if (tile.Type == FloorTileType.Normal && tile.Number != 0)
                                {
                                    answere = false;
                                }
                            }
                            _isDuringGame = false;
                            _gameFinishSuccesfull = answere;
                        }
                        _moveCommands.Add(new MoveCommand(_playerVirtualPoint, _playerVirtualPointDestination, modFloorTilesBefore, modFloorTilesAfter));
                        _moveCommandsIndex = _moveCommands.Count;
                        
                    }
                    if (_playerUndoMove == true)
                    {
                        _moveCommandsIndex -= 1;
                        _playerUndoMove = false;
                    }
                    if (_playerRedoMove == true)
                    {   
                        List<FloorTile> floorTiles = _moveCommands[_moveCommandsIndex].GetModifiedFloorTileAfter();
                        foreach (FloorTile floorTile in floorTiles)
                        {
                            int indexTile = _floorTiles.FindIndex(item => item.PosX == floorTile.PosX && item.PosY == floorTile.PosY);
                            _floorTiles[indexTile] = floorTile;
                        }
                        _moveCommandsIndex += 1;
                        _playerRedoMove = false;
                    }
                    _playerVirtualPoint = _playerVirtualPointDestination;
                }
            }
        }
        private Vector2 GetLevelStart()
        {
            int offestX = -_floorTiles.Min(posX => posX.PosX);
            int offsetY = _floorTiles.Max(posY => posY.PosY);
            return new Vector2(offestX, offsetY);
        }
        private void StartNewGame()
        {
            _floorTiles = new List<FloorTile>();
            _currentFloorTile = new FloorTile();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("LevelBasic/0_Basic_01.xml");
            string xmlString = xmlDocument.OuterXml;
            using (StringReader read = new StringReader(xmlString))
            {
                Type outType = typeof(List<FloorTile>);

                XmlSerializer serializer = new XmlSerializer(outType);
                using (XmlReader reader = new XmlTextReader(read))
                {
                    _floorTiles = (List<FloorTile>)serializer.Deserialize(reader);
                    reader.Close();
                }
                read.Close();
            }
            _currentFloorTile = _floorTiles.Find(pos => pos.PosX == 0 && pos.PosY == 0);
            _currentFloorTile.Number -= 1;
            int indexTile = _floorTiles.FindIndex(item => item.PosX == _currentFloorTile.PosX && item.PosY == _currentFloorTile.PosY);
            _floorTiles[indexTile] = _currentFloorTile;

            _levelStart = GetLevelStart();

            _playerPosition = new Vector2(100 * _levelStart.X, 100 * _levelStart.Y);
            _playerVirtualPoint = new Point(0, 0);
            _playerVirtualPointDestination = new Point(0, 0);

            _isDuringGame = true;
            _playerPerformMove = false;
            _playerMoveInital = false;

            _moveCommands = new List<MoveCommand>();
            _moveCommandsIndex = 0;

            _playerUndoMove = false;
            _playerRedoMove = false;
        }
    }
}