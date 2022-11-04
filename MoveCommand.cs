using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SlidingTile_MonoGame
{
    internal class MoveCommand
    {
        private Point _startPoint;
        private Point _endPoint;
        private List<FloorTile> _modifiedFloorTileBefore;
        private List<FloorTile> _modifiedFloorTileAfter;
        public MoveCommand(Point startPoint, Point endPoint, List<FloorTile> modifiedFloorTileBefore, List<FloorTile> modifiedFloorTileAfter)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
            _modifiedFloorTileBefore = modifiedFloorTileBefore;
            _modifiedFloorTileAfter = modifiedFloorTileAfter;
        }
        public Point GetStartPoint()
        {
            return _startPoint;
        }
        public Point GetEndPoint()
        {
            return _endPoint;
        }
        public List<FloorTile> GetModifiedFloorTileBefore()
        {
            return _modifiedFloorTileBefore;
        }
        public List<FloorTile> GetModifiedFloorTileAfter()
        {
            return _modifiedFloorTileAfter;
        }
    }
}
