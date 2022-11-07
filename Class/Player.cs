using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace SlidingTile_MonoGame.Class
{
    public class Player : Sprite
    {
        KeyboardState currentKS, previousKS;
        GamePadState currentGPS, previousGPS;

        Game1 _game1;
        private Point _playerVirtualPoint;
        List<int> _routTilesIndexes;
        int _moveCommandsIndex;
        List<MoveCommand> _moveCommands;
        private float _stepDistans;
        private Point _levelStart;

        private SpriteFont _debugGame;
        private Vector2 _debugMoveCountPosition, _debugMoveListPosition;

        public Player(Texture2D texture, Game1 game1, Point levelStart, SpriteFont debugGame, float stepDistans) : base(texture)
        {
            _game1 = game1;
            _levelStart = levelStart;
            _debugGame = debugGame;
            _moveCommands = new List<MoveCommand>();
            _moveCommandsIndex = 0;
            _debugMoveCountPosition = new Vector2(20, Game1.ScreenHeight -
                _debugGame.MeasureString("Player move commands: counts").Y - 20);
            _debugMoveListPosition = new Vector2(Game1.ScreenWidth -
                _debugGame.MeasureString("[99] Start [00,00], End [00,00], Floor tile mod before count: 00").X - 20, 20);
            _stepDistans = stepDistans;
            UpdatePlayerPosition(_playerVirtualPoint);
        }
        public override void Update(float gameTime)
        {
            previousKS = currentKS;
            currentKS = Keyboard.GetState();
            previousGPS = currentGPS;
            currentGPS = GamePad.GetState(PlayerIndex.One);

            if ((currentKS.IsKeyDown(Keys.Up) && previousKS.IsKeyUp(Keys.Up)) ||
                    (currentKS.IsKeyDown(Keys.W) && previousKS.IsKeyUp(Keys.W)) ||
                    (currentGPS.IsButtonDown(Buttons.DPadUp) && previousGPS.IsButtonUp(Buttons.DPadUp)))
            {
                MovePlayer(new Point(0, 1));
            }

            if ((currentKS.IsKeyDown(Keys.Down) && previousKS.IsKeyUp(Keys.Down)) ||
                (currentKS.IsKeyDown(Keys.S) && previousKS.IsKeyUp(Keys.S)) ||
                (currentGPS.IsButtonDown(Buttons.DPadDown) && previousGPS.IsButtonUp(Buttons.DPadDown)))
            {
                MovePlayer(new Point(0, -1));
            }

            if ((currentKS.IsKeyDown(Keys.Left) && previousKS.IsKeyUp(Keys.Left)) ||
                (currentKS.IsKeyDown(Keys.A) && previousKS.IsKeyUp(Keys.A)) ||
                (currentGPS.IsButtonDown(Buttons.DPadLeft) && previousGPS.IsButtonUp(Buttons.DPadLeft)))
            {
                MovePlayer(new Point(-1, 0));
            }

            if ((currentKS.IsKeyDown(Keys.Right) && previousKS.IsKeyUp(Keys.Right)) ||
                (currentKS.IsKeyDown(Keys.D) && previousKS.IsKeyUp(Keys.D)) ||
                (currentGPS.IsButtonDown(Buttons.DPadRight) && previousGPS.IsButtonUp(Buttons.DPadRight)))
            {
                MovePlayer(new Point(1, 0));
            }

            if ((currentKS.IsKeyDown(Keys.U) && previousKS.IsKeyUp(Keys.U)) ||
                (currentGPS.IsButtonDown(Buttons.LeftShoulder) && previousGPS.IsButtonUp(Buttons.LeftShoulder)))
            {
                UndoPlayer();
            }

            if ((currentKS.IsKeyDown(Keys.R) && previousKS.IsKeyUp(Keys.R)) ||
                (currentGPS.IsButtonDown(Buttons.RightShoulder) && previousGPS.IsButtonUp(Buttons.RightShoulder)))
            {
                RedoPlayer();
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_debugGame, "Player current virtual location: [" + _playerVirtualPoint.X + "," + _playerVirtualPoint.Y + "]", _debugMoveCountPosition + new Vector2(0, -30), Color.White);
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
                spriteBatch.DrawString(_debugGame, "[" + i.ToString() + "] Start [" + start.X.ToString() + "," + start.Y.ToString() + "], End [" + end.X.ToString() +
                    "," + end.Y.ToString() + "], Floor tile mod before count: " + _moveCommands[i].GetModifiedFloorTileBefore().Count.ToString(), _debugMoveListPosition + verticalOffset, currentIndex);
            }
            base.Draw(spriteBatch);
        }
        private List<int> GetPlayerRoutIndexes(Point moveDir)
        {
            List<int> floorTiles = new List<int>();
            List<FloorTile> clonedList = new List<FloorTile>();
            for (int i = 0; i < _game1._floorTiles.Count; i++)
            {
                FloorTile floorTile = new FloorTile()
                {
                    PosX = _game1._floorTiles[i].PosX,
                    PosY = _game1._floorTiles[i].PosY,
                    Number = _game1._floorTiles[i].Number,
                    Type = _game1._floorTiles[i].Type
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
                        _playerVirtualPoint.X = _game1._floorTiles[_routTilesIndexes[i]].PosX;
                        _playerVirtualPoint.Y = _game1._floorTiles[_routTilesIndexes[i]].PosY;
                        UpdatePlayerPosition(_playerVirtualPoint);

                        modFloorTilesBefore.Add(new FloorTile()
                        {
                            Number = _game1._floorTiles[_routTilesIndexes[i]].Number,
                            PosX = _game1._floorTiles[_routTilesIndexes[i]].PosX,
                            PosY = _game1._floorTiles[_routTilesIndexes[i]].PosY,
                            Type = _game1._floorTiles[_routTilesIndexes[i]].Type
                        });

                        _game1._floorTiles[_routTilesIndexes[i]].Number--;
                        modFloorTilesAfter.Add(new FloorTile()
                        {
                            Number = _game1._floorTiles[_routTilesIndexes[i]].Number,
                            PosX = _game1._floorTiles[_routTilesIndexes[i]].PosX,
                            PosY = _game1._floorTiles[_routTilesIndexes[i]].PosY,
                            Type = _game1._floorTiles[_routTilesIndexes[i]].Type
                        });

                        if (_game1._floorTiles[_routTilesIndexes[i]].Type == FloorTileType.Finish)
                        {
                            bool answere = true;
                            foreach (FloorTile tile in _game1._floorTiles)
                            {
                                if ((tile.Type == FloorTileType.Normal || tile.Type == FloorTileType.Ice) && tile.Number != 0)
                                {
                                    answere = false;
                                }
                            }
                            _game1._isDuringGame = false;
                            _game1._gameFinishSuccesfull = answere;
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
            Position = new Vector2(_stepDistans * _levelStart.X, _stepDistans * _levelStart.Y) +
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
                    List<FloorTile> tilesFinded = _game1._floorTiles.FindAll(tile => tile.PosX == tilesBefore[i].PosX && tile.PosY == tilesBefore[i].PosY);
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
                    List<FloorTile> tilesFinded = _game1._floorTiles.FindAll(tile => tile.PosX == tilesAfter[i].PosX && tile.PosY == tilesAfter[i].PosY);
                    for (int j = 0; j < tilesFinded.Count; j++)
                    {
                        tilesFinded[j].Number = tilesAfter[i].Number;
                    }
                }

                UpdatePlayerPosition(moveCommand.GetEndPoint());

                _moveCommandsIndex++;
            }
        }
    }
}
