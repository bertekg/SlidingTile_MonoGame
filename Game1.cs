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

        private Texture2D _playerSprite;

        private Vector2 _playerPosition;
        private Point _playerVirtualPoint;
        //private Point _playerVirtualPointDestination;

        //private float _currChangeValue;

        private bool _isDuringGame;
        //private bool _playerPerformMove;
        //private bool _playerMoveInital;
        //private float _playerMoveSpeed;
        //private double _timeSignleMoveMax, _timeMoveCurrent, _timeTotalMove;
        private float _stepDistans;
        //private Vector2 _moveVerse, _playerPosInit;

        private Texture2D _floorTileNormalSprite, _floorTileIceSprite;

        private SpriteFont _digitFloor;
        private SpriteFont _debugGame;
        private SpriteFont _gameEnd;

        List<FloorTile> _floorTiles;
        Vector2 _levelStart;
        FloorTile _currentFloorTile;

        List<MoveCommand> _moveCommands;
        int _moveCommandsIndex;
        //bool _playerUndoMove;
        //bool _playerRedoMove;

        bool _gameFinishSuccesfull;
        private Texture2D _endGamePanelTexture2D;

        private Vector2 _endGamePanelPosition, _endGamePanelGameOverLoc;
        private Vector2 _endGamePanelFinishStatusWinLoc, _endGamePanelFinishStatusLoseLoc, _endGamePanelSpaceToStartAgainLoc;

        private Vector2 _debugMoveCountPosition, _debugMoveListPosition;

        private string _endGamePanelFinishGameOver, _endGamePanelFinishStatusLose;
        private string _endGamePanelFinishStatusWin, _endGamePanelSpaceToStartAgainText;

        List<int> _routTilesIndexes;

        KeyboardState currentKS, previousKS;
        GamePadState currentGPS, previousGPS;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        protected override void Initialize()
        {
            Window.Title = "Sliding Tile - MonoGame (0.3.3 - 2022.11.06)";

            StartNewGame();

            //_timeSignleMoveMax = 0.3d;
            //_timeMoveCurrent = 0.0d;
            _stepDistans = 100.0f;

            _endGamePanelFinishGameOver = "Game Over!!!";
            _endGamePanelFinishStatusWin = "You WIN :)";
            _endGamePanelFinishStatusLose = "You LOSE :(";
            _endGamePanelSpaceToStartAgainText = "[Space, START] - Restart game";

            //_moveVerse = new Vector2();

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
            previousKS = currentKS;
            currentKS = Keyboard.GetState();
            previousGPS = currentGPS;
            currentGPS = GamePad.GetState(PlayerIndex.One);

            if (currentKS.IsKeyDown(Keys.Escape) || currentGPS.IsButtonDown(Buttons.Back))
                Exit();

            if (_isDuringGame)
            {
                if ((currentKS.IsKeyDown(Keys.Up) && previousKS.IsKeyUp(Keys.Up)) ||
                    (currentKS.IsKeyDown(Keys.W) && previousKS.IsKeyUp(Keys.W)) ||
                    (currentGPS.IsButtonDown(Buttons.DPadUp) && previousGPS.IsButtonUp(Buttons.DPadUp)))
                    //&& _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
                {
                    //_moveVerse = new Vector2(0.0f, -1.0f);
                    MovePlayer(new Point(0, 1));
                    //_playerPerformMove = true;
                }

                if ((currentKS.IsKeyDown(Keys.Down) && previousKS.IsKeyUp(Keys.Down)) ||
                    (currentKS.IsKeyDown(Keys.S) && previousKS.IsKeyUp(Keys.S)) ||
                    (currentGPS.IsButtonDown(Buttons.DPadDown) && previousGPS.IsButtonUp(Buttons.DPadDown)))
                    //&& _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
                {
                    //_moveVerse = new Vector2(0.0f, 1.0f);
                    MovePlayer(new Point(0, -1));
                    //_playerPerformMove = true;
                }

                if ((currentKS.IsKeyDown(Keys.Left) && previousKS.IsKeyUp(Keys.Left)) ||
                    (currentKS.IsKeyDown(Keys.A) && previousKS.IsKeyUp(Keys.A)) ||
                    (currentGPS.IsButtonDown(Buttons.DPadLeft) && previousGPS.IsButtonUp(Buttons.DPadLeft)))
                    //&& _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
                {
                    //_moveVerse = new Vector2(-1.0f, 0.0f);
                    MovePlayer(new Point(-1, 0));
                    //_playerPerformMove = true;
                }

                if ((currentKS.IsKeyDown(Keys.Right) && previousKS.IsKeyUp(Keys.Right)) ||
                    (currentKS.IsKeyDown(Keys.D) && previousKS.IsKeyUp(Keys.D)) ||
                    (currentGPS.IsButtonDown(Buttons.DPadRight) && previousGPS.IsButtonUp(Buttons.DPadRight)))
                    //&& _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
                {
                    //_moveVerse = new Vector2(1.0f, 0.0f);
                    MovePlayer(new Point(1,0));
                    //_playerPerformMove = true;
                }

                if ((currentKS.IsKeyDown(Keys.U) && previousKS.IsKeyUp(Keys.U)) ||
                    (currentGPS.IsButtonDown(Buttons.LeftShoulder) && previousGPS.IsButtonUp(Buttons.LeftShoulder)))
                    //&& _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
                {
                    UndoPlayer();
                    //_playerUndoMove = true;
                    //_playerPerformMove = true;
                }

                if ((currentKS.IsKeyDown(Keys.R) && previousKS.IsKeyUp(Keys.R)) ||
                    (currentGPS.IsButtonDown(Buttons.RightShoulder) && previousGPS.IsButtonUp(Buttons.RightShoulder)))
                    //&& _playerPerformMove == false && _playerUndoMove == false && _playerRedoMove == false)
                {
                    RedoPlayer();
                    //_playerRedoMove = true;
                    //_playerPerformMove = true;
                }
                //MovePlayer(gameTime.ElapsedGameTime.TotalSeconds);
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

            for (int i = 0; i < _floorTiles.Count; i++)
            {
                if (_floorTiles[i].Type == FloorTileType.Normal && _floorTiles[i].Number == 0) continue;
                if (_floorTiles[i].Type == FloorTileType.Ice && _floorTiles[i].Number == 0) continue;
                Vector2 finalPosition = new Vector2(100*((int)_levelStart.X + _floorTiles[i].PosX), 100 * ((int)_levelStart.Y - _floorTiles[i].PosY));
                if (_floorTiles[i].Type != FloorTileType.Ice)
                {
                    _spriteBatch.Draw(_floorTileNormalSprite, finalPosition, Color.White);
                }
                else
                {
                    _spriteBatch.Draw(_floorTileIceSprite, finalPosition, Color.White);
                }
                
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
                        textInside = _floorTiles[i].Number.ToString();
                        colorText = Color.Black;
                        vectorTextOffset = new Vector2(35, 15);
                        break;
                    default:
                        break;
                }
                _spriteBatch.DrawString(_digitFloor, textInside, finalPosition + vectorTextOffset, colorText);
            }

            _spriteBatch.DrawString(_debugGame, "Player move commands: counts " + _moveCommands.Count.ToString() + ", index " + _moveCommandsIndex.ToString(), _debugMoveCountPosition, Color.White);
            _spriteBatch.DrawString(_debugGame, "Player current virtual location: [" + _playerVirtualPoint.X + "," + _playerVirtualPoint.Y + "]", _debugMoveCountPosition + new Vector2(0, -30), Color.White);
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

            _spriteBatch.Draw(_playerSprite, _playerPosition, Color.White);

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
        private List<int> GetPlayerRoutIndexes(Point moveDir)
        {
            List<int> floorTiles = new List<int>();
            List<FloorTile> clonedList = new List<FloorTile>();
            for (int i = 0; i < _floorTiles.Count; i++)
            {
                FloorTile floorTile = new FloorTile()
                {
                    PosX = _floorTiles[i].PosX,
                    PosY = _floorTiles[i].PosY,
                    Number = _floorTiles[i].Number,
                    Type = _floorTiles[i].Type
                };
                clonedList.Add(floorTile);
            };
            bool duringSerching = true;
            Point pointForDetection = _playerVirtualPoint;
            while (duringSerching)
            {
                Point checkThisFloor = pointForDetection + moveDir;
                FloorTile floorTile = clonedList.Find(pos => pos.PosX == checkThisFloor.X && pos.PosY == checkThisFloor.Y);
                if (floorTile != null)
                {
                    if (floorTile.Number > 0 || floorTile.Type == FloorTileType.Finish)
                    {
                        int index = clonedList.IndexOf(floorTile);
                        switch (floorTile.Type)
                        {
                            case FloorTileType.None:
                                duringSerching = false;
                                break;
                            case FloorTileType.Finish:
                                floorTiles.Add(index);
                                duringSerching = false;
                                break;
                            case FloorTileType.Normal:
                                floorTiles.Add(index);
                                clonedList[index].Number--;
                                duringSerching = false;
                                break;
                            case FloorTileType.Ice:
                                floorTiles.Add(index);
                                clonedList[index].Number--;
                                pointForDetection = checkThisFloor;
                                break;
                            default:
                                duringSerching = false;
                                break;
                        }
                    }
                    else
                    {
                        duringSerching = false;
                    }                    
                }
                else
                {
                    duringSerching = false;
                }
            }           
            return floorTiles;
        }
        private void MovePlayer(Point moveDir)
        {
            _routTilesIndexes = GetPlayerRoutIndexes(moveDir);
            if (_routTilesIndexes != null)
            {
                if (_routTilesIndexes.Count > 0)
                {
                    Point playerStart = _playerVirtualPoint;
                    List<FloorTile> modFloorTilesBefore = new List<FloorTile>();
                    List<FloorTile> modFloorTilesAfter = new List<FloorTile>();
                    for (int i = 0; i < _routTilesIndexes.Count; i++)
                    {                       
                        _playerVirtualPoint.X = _floorTiles[_routTilesIndexes[i]].PosX;
                        _playerVirtualPoint.Y = _floorTiles[_routTilesIndexes[i]].PosY;
                        UpdatePlayerPosition(_playerVirtualPoint);

                        modFloorTilesBefore.Add(new FloorTile() { 
                            Number = _floorTiles[_routTilesIndexes[i]].Number,
                            PosX = _floorTiles[_routTilesIndexes[i]].PosX,
                            PosY = _floorTiles[_routTilesIndexes[i]].PosY,
                            Type = _floorTiles[_routTilesIndexes[i]].Type 
                        });

                        _floorTiles[_routTilesIndexes[i]].Number--;
                        modFloorTilesAfter.Add(new FloorTile()
                        {
                            Number = _floorTiles[_routTilesIndexes[i]].Number,
                            PosX = _floorTiles[_routTilesIndexes[i]].PosX,
                            PosY = _floorTiles[_routTilesIndexes[i]].PosY,
                            Type = _floorTiles[_routTilesIndexes[i]].Type
                        });

                        if (_floorTiles[_routTilesIndexes[i]].Type == FloorTileType.Finish)
                        {
                            bool answere = true;
                            foreach (FloorTile tile in _floorTiles)
                            {
                                if ((tile.Type == FloorTileType.Normal || tile.Type == FloorTileType.Ice) && tile.Number != 0)
                                {
                                    answere = false;
                                }
                            }
                            _isDuringGame = false;
                            _gameFinishSuccesfull = answere;
                            break;
                        }
                    }
                    if (_moveCommandsIndex < _moveCommands.Count)
                    {
                        int howManyDelete = _moveCommands.Count - _moveCommandsIndex;
                        for (int i = 0; i < howManyDelete; i++)
                        {
                            _moveCommands.RemoveAt(_moveCommands.Count - 1);
                        }
                    }
                    _moveCommands.Add(new MoveCommand(playerStart, _playerVirtualPoint, modFloorTilesBefore, modFloorTilesAfter));
                    _moveCommandsIndex = _moveCommands.Count;
                }
            }
        }
        private void UpdatePlayerPosition(Point destination)
        {
            _playerVirtualPoint = destination;
            _playerPosition = new Vector2(_stepDistans * _levelStart.X, _stepDistans * _levelStart.Y) +
                            new Vector2(_stepDistans * _playerVirtualPoint.X, -_stepDistans * _playerVirtualPoint.Y);
        }
        private void UndoPlayer()
        {
            if (_moveCommandsIndex > 0)
            {
                MoveCommand moveCommand = _moveCommands[_moveCommandsIndex - 1];
                List<FloorTile> tilesBefore = moveCommand.GetModifiedFloorTileBefore();
                for (int i = 0; i < tilesBefore.Count; i++)
                {
                    List<FloorTile> tilesFinded = _floorTiles.FindAll(tile => tile.PosX == tilesBefore[i].PosX && tile.PosY == tilesBefore[i].PosY);
                    for (int j = 0; j < tilesFinded.Count; j++)
                    {
                        tilesFinded[j].Number = tilesBefore[i].Number;
                    }
                }
                
                UpdatePlayerPosition(moveCommand.GetStartPoint());

                _moveCommandsIndex--;
            }
        }
        private void RedoPlayer()
        {
            if (_moveCommandsIndex < _moveCommands.Count)
            {
                MoveCommand moveCommand = _moveCommands[_moveCommandsIndex];
                List<FloorTile> tilesAfter = moveCommand.GetModifiedFloorTileAfter();
                for (int i = 0; i < tilesAfter.Count; i++)
                {
                    List<FloorTile> tilesFinded = _floorTiles.FindAll(tile => tile.PosX == tilesAfter[i].PosX && tile.PosY == tilesAfter[i].PosY);
                    for (int j = 0; j < tilesFinded.Count; j++)
                    {
                        tilesFinded[j].Number = tilesAfter[i].Number;
                    }
                }

                UpdatePlayerPosition(moveCommand.GetEndPoint());

                _moveCommandsIndex++;
            }
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
            xmlDocument.Load("Levels/2_Ice/2_Ice_03.xml");
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
            //_playerVirtualPointDestination = new Point(0, 0);

            _isDuringGame = true;
            //_playerPerformMove = false;
            //_playerMoveInital = false;

            _moveCommands = new List<MoveCommand>();
            _moveCommandsIndex = 0;

            //_playerUndoMove = false;
            //_playerRedoMove = false;
        }
    }
}