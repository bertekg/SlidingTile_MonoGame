using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using SlidingTile_MonoGame.Class;

namespace SlidingTile_MonoGame
{
    public class Game1 : Game
    {
        public static int ScreenWidth = 1900;
        public static int ScreenHeight = 980;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _playerSprite;

        public bool _isDuringGame;
        private float _stepDistans;

        private Texture2D _floorTileNormalSprite, _floorTileIceSprite, _floorTileStaticSprite, _floorTilePortalSprite;
        private Texture2D _floorTileSpringUpSprite, _floorTileSpringLeftSprite, _floorTileSpringDownSprite, _floorTileSpringRightSprite;

        private SpriteFont _digitFloor;
        private SpriteFont _debugGame;
        private SpriteFont _gameEnd;

        public List<FloorTile> _floorTiles;
        Point _levelStart;
        FloorTile _currentFloorTile;

        public bool _gameFinishSuccesfull;
        private Texture2D _endGamePanelTexture2D;

        private Vector2 _endGamePanelPosition, _endGamePanelGameOverLoc;
        private Vector2 _endGamePanelFinishStatusWinLoc, _endGamePanelFinishStatusLoseLoc, _endGamePanelSpaceToStartAgainLoc;

        private string _endGamePanelFinishGameOver, _endGamePanelFinishStatusLose;
        private string _endGamePanelFinishStatusWin, _endGamePanelSpaceToStartAgainText;

        private List<Sprite> _sprites;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        protected override void Initialize()
        {
            Window.Title = "Sliding Tile - MonoGame (0.4.2 - 2022.11.27)";

            //_timeSignleMoveMax = 0.3d;
            _stepDistans = 100.0f;

            _endGamePanelFinishGameOver = "Game Over!!!";
            _endGamePanelFinishStatusWin = "You WIN :)";
            _endGamePanelFinishStatusLose = "You LOSE :(";
            _endGamePanelSpaceToStartAgainText = "[Space, START] - Restart game";

            _graphics.PreferredBackBufferWidth = ScreenWidth;
            _graphics.PreferredBackBufferHeight = ScreenHeight;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();

            base.Initialize();
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _playerSprite = Content.Load<Texture2D>("sprites/player");
            _floorTileNormalSprite = Content.Load<Texture2D>("sprites/floorTile");
            _floorTileIceSprite = Content.Load<Texture2D>("sprites/floorTile_Ice");
            _floorTileStaticSprite = Content.Load<Texture2D>("sprites/florTile_Static");
            _floorTilePortalSprite = Content.Load<Texture2D>("sprites/florTile_Portal");
            _floorTileSpringUpSprite = Content.Load<Texture2D>("sprites/floorTile_SpringUp");
            _floorTileSpringLeftSprite = Content.Load<Texture2D>("sprites/floorTile_SpringLeft");
            _floorTileSpringDownSprite = Content.Load<Texture2D>("sprites/floorTile_SpringDown");
            _floorTileSpringRightSprite = Content.Load<Texture2D>("sprites/floorTile_SpringRight");
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

            StartNewGame();
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState currentKS = Keyboard.GetState();
            GamePadState currentGPS = GamePad.GetState(PlayerIndex.One);

            if (currentKS.IsKeyDown(Keys.Escape) || currentGPS.IsButtonDown(Buttons.Back))
                Exit();

            if (_isDuringGame)
            {
                foreach (var sprite in _sprites)
                    sprite.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            else
            {
                if (currentKS.IsKeyDown(Keys.Space) || currentGPS.IsButtonDown(Buttons.Start))
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

            foreach (var sprite in _sprites)
                sprite.Draw(_spriteBatch);

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

        private Point GetLevelStart()
        {
            int offestX = -_floorTiles.Min(posX => posX.PosX);
            int offsetY = _floorTiles.Max(posY => posY.PosY);
            return new Point(offestX, offsetY);
        }
        private void StartNewGame()
        {
            _floorTiles = new List<FloorTile>();
            _currentFloorTile = new FloorTile();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("Levels/T_Variant/Test4Springs.xml");
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

            _sprites = new List<Sprite>();            
            for (int i = 0; i < _floorTiles.Count; i++)
            {
                Tile tile = null;
                switch (_floorTiles[i].Type)
                {
                    case FloorTileType.None:
                        break;
                    case FloorTileType.Finish:
                        tile = new Tile(_floorTileNormalSprite);
                        break;
                    case FloorTileType.Normal:
                        tile = new Tile(_floorTileNormalSprite);
                        break;
                    case FloorTileType.Ice:
                        tile = new Tile(_floorTileIceSprite);
                        break;
                    case FloorTileType.Static:
                        tile = new Tile(_floorTileStaticSprite);
                        break;
                    case FloorTileType.Portal:
                        tile = new Tile(_floorTilePortalSprite);
                        break;
                    case FloorTileType.Spring:
                        switch (_floorTiles[i].Spring)
                        {
                            case SpringDirection.Up:
                                tile = new Tile(_floorTileSpringUpSprite);
                                break;
                            case SpringDirection.Left:
                                tile = new Tile(_floorTileSpringLeftSprite);
                                break;
                            case SpringDirection.Down:
                                tile = new Tile(_floorTileSpringDownSprite);
                                break;
                            case SpringDirection.Right:
                                tile = new Tile(_floorTileSpringRightSprite);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
                tile.FloorTile = _floorTiles[i];
                tile.DigitFont = _digitFloor;
                tile.Position = new Vector2(_stepDistans * ((int)_levelStart.X + _floorTiles[i].PosX), _stepDistans * ((int)_levelStart.Y - _floorTiles[i].PosY));
                _sprites.Add(tile);
            }
            _sprites.Add(new Player(_playerSprite, this, _levelStart, _debugGame, _stepDistans));

            _isDuringGame = true;
        }
        #region comment
        //private void MovePlayer(double totalSecounds)
        //{
        //    if(_playerPerformMove == true && _playerMoveInital == false && _playerUndoMove == false && _playerRedoMove == false)
        //    {
        //        //_routTilesIndexes = GetPlayerRoutIndexes();
        //        if (_routTilesIndexes != null)
        //        {
        //            if (_routTilesIndexes.Count > 0)
        //            {
        //                for (int i = 0; i < _routTilesIndexes.Count; i++)
        //                {
        //                    _playerVirtualPoint.X = _floorTiles[_routTilesIndexes[i]].PosX;
        //                    _playerVirtualPoint.Y = _floorTiles[_routTilesIndexes[i]].PosY;
        //                    _playerPosition = new Vector2(_stepDistans * _levelStart.X, _stepDistans * _levelStart.Y) +
        //                        new Vector2(_stepDistans * _playerVirtualPoint.X, -_stepDistans * _playerVirtualPoint.Y);
        //                    _floorTiles[_routTilesIndexes[i]].Number--;
        //                    if (_floorTiles[_routTilesIndexes[i]].Type == FloorTileType.Finish)
        //                    {
        //                        bool answere = true;
        //                        foreach (FloorTile tile in _floorTiles)
        //                        {
        //                            if ((tile.Type == FloorTileType.Normal || tile.Type == FloorTileType.Ice) && tile.Number != 0)
        //                            {
        //                                answere = false;
        //                            }
        //                        }
        //                        _isDuringGame = false;
        //                        _gameFinishSuccesfull = answere;
        //                        _playerPerformMove = false;
        //                        break;
        //                    }
        //                    _playerPerformMove = false;
        //                }
        //                //_playerMoveSpeed = _stepDistans / (float)_timeSignleMoveMax;
        //                //_currentFloorTile = _floorTiles[_routTilesIndexes[0]];
        //                //_timeMoveCurrent = 0.0d;
        //                //_playerMoveInital = true;
        //                //_playerPosInit = _playerPosition;
        //            }
        //            else
        //            {
        //                _playerPerformMove = false;
        //            }
        //        }
        //        else
        //        {
        //            _playerPerformMove = false;
        //        }
        //    }
        //    _playerPerformMove = false;
        //    //if (_playerPerformMove == true && _playerMoveInital == false && _playerUndoMove == true && _playerRedoMove == false)
        //    //{
        //    //    if (_moveCommandsIndex > 0)
        //    //    {
        //    //        MoveCommand moveCommand = _moveCommands[_moveCommandsIndex - 1];
        //    //        _playerVirtualPointDestination = moveCommand.GetStartPoint();
        //    //        _playerMoveSpeed = _stepDistans / (float)_timeSignleMoveMax;
        //    //        _timeMoveCurrent = 0.0d;
        //    //        _playerMoveInital = true;
        //    //        _playerPosInit = _playerPosition;
        //    //        _moveVerse = new Vector2(moveCommand.GetStartPoint().X - moveCommand.GetEndPoint().X, -(moveCommand.GetStartPoint().Y - moveCommand.GetEndPoint().Y));

        //    //        List<FloorTile> floorTiles = _moveCommands[_moveCommandsIndex - 1].GetModifiedFloorTileBefore();
        //    //        foreach (FloorTile floorTile in floorTiles)
        //    //        {
        //    //            int indexTile = _floorTiles.FindIndex(item => item.PosX == floorTile.PosX && item.PosY == floorTile.PosY);
        //    //            _floorTiles[indexTile] = floorTile;
        //    //        }
        //    //    }
        //    //    else
        //    //    {
        //    //        _playerPerformMove = false;
        //    //        _playerUndoMove = false;
        //    //    }
        //    //}
        //    //if (_playerPerformMove == true && _playerMoveInital == false && _playerUndoMove == false && _playerRedoMove == true)
        //    //{
        //    //    if (_moveCommandsIndex < _moveCommands.Count)
        //    //    {
        //    //        MoveCommand moveCommand = _moveCommands[_moveCommandsIndex];
        //    //        _playerVirtualPointDestination = moveCommand.GetEndPoint();
        //    //        _playerMoveSpeed = _stepDistans / (float)_timeSignleMoveMax;
        //    //        _timeMoveCurrent = 0.0d;
        //    //        _playerMoveInital = true;
        //    //        _playerPosInit = _playerPosition;
        //    //        _moveVerse = new Vector2(moveCommand.GetEndPoint().X - moveCommand.GetStartPoint().X, -(moveCommand.GetEndPoint().Y - moveCommand.GetStartPoint().Y));
        //    //    }
        //    //    else
        //    //    {
        //    //        _playerPerformMove = false;
        //    //        _playerRedoMove = false;
        //    //    }
        //    //}

        //    //_currChangeValue = (float)(_playerMoveSpeed * totalSecounds);

        //    //if (_playerPerformMove == true & _playerMoveInital == true)
        //    //{
        //    //    if (_timeMoveCurrent <= _timeSignleMoveMax)
        //    //    {
        //    //        _playerPosition += new Vector2(_currChangeValue * _moveVerse.X, _currChangeValue * _moveVerse.Y);
        //    //        _timeMoveCurrent += totalSecounds;
        //    //    }
        //    //    else
        //    //    {
        //    //        _playerPosition = new Vector2(_playerPosInit.X + (_stepDistans * _moveVerse.X), _playerPosInit.Y + (_stepDistans * _moveVerse.Y));
        //    //        _playerMoveInital = false;
        //    //        _playerPerformMove = false;

        //    //        if (_playerUndoMove == false && _playerRedoMove == false)
        //    //        {
        //    //            if (_moveCommandsIndex < _moveCommands.Count)
        //    //            {
        //    //                int howManyDelete = _moveCommands.Count - _moveCommandsIndex;
        //    //                for (int i = 0; i < howManyDelete; i++)
        //    //                {
        //    //                    _moveCommands.RemoveAt(_moveCommands.Count - 1);
        //    //                }
        //    //            }
        //    //            List<FloorTile> modFloorTilesBefore = new List<FloorTile>();
        //    //            List<FloorTile> modFloorTilesAfter = new List<FloorTile>();
        //    //            if (_currentFloorTile.Type == FloorTileType.Normal)
        //    //            {
        //    //                int indexTile = _floorTiles.FindIndex(item => item.PosX == _currentFloorTile.PosX && item.PosY == _currentFloorTile.PosY);
        //    //                modFloorTilesBefore.Add(new FloorTile() { Number = _currentFloorTile.Number, PosX = _currentFloorTile.PosX, PosY = _currentFloorTile.PosY, Type = _currentFloorTile.Type});
        //    //                _currentFloorTile.Number -= 1;
        //    //                _floorTiles[indexTile].Number = _currentFloorTile.Number;
        //    //                modFloorTilesAfter.Add(new FloorTile() { Number = _currentFloorTile.Number, PosX = _currentFloorTile.PosX, PosY = _currentFloorTile.PosY, Type = _currentFloorTile.Type });
        //    //            }
        //    //            else if(_currentFloorTile.Type == FloorTileType.Finish)
        //    //            {
        //    //                bool answere = true;
        //    //                foreach (FloorTile tile in _floorTiles)
        //    //                {
        //    //                    if (tile.Type == FloorTileType.Normal && tile.Number != 0)
        //    //                    {
        //    //                        answere = false;
        //    //                    }
        //    //                }
        //    //                _isDuringGame = false;
        //    //                _gameFinishSuccesfull = answere;
        //    //            }
        //    //            _moveCommands.Add(new MoveCommand(_playerVirtualPoint, _playerVirtualPointDestination, modFloorTilesBefore, modFloorTilesAfter));
        //    //            _moveCommandsIndex = _moveCommands.Count;

        //    //        }
        //    //        if (_playerUndoMove == true)
        //    //        {
        //    //            _moveCommandsIndex -= 1;
        //    //            _playerUndoMove = false;
        //    //        }
        //    //        if (_playerRedoMove == true)
        //    //        {   
        //    //            List<FloorTile> floorTiles = _moveCommands[_moveCommandsIndex].GetModifiedFloorTileAfter();
        //    //            foreach (FloorTile floorTile in floorTiles)
        //    //            {
        //    //                int indexTile = _floorTiles.FindIndex(item => item.PosX == floorTile.PosX && item.PosY == floorTile.PosY);
        //    //                _floorTiles[indexTile] = floorTile;
        //    //            }
        //    //            _moveCommandsIndex += 1;
        //    //            _playerRedoMove = false;
        //    //        }
        //    //        _playerVirtualPoint = _playerVirtualPointDestination;
        //    //    }
        //    //}
        //}
        #endregion
    }
}