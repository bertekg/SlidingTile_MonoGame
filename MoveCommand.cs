using Microsoft.Xna.Framework;
using System.Collections.Generic;
using static SlidingTile_MonoGame.Game1;

namespace SlidingTile_MonoGame
{
    internal class MoveCommand
    {
        private Point startPoint;
        private Point endPoint;
        private List<FloorTile> modifiedFloorTileBefore;
        private List<FloorTile> modifiedFloorTileAfter;
        public MoveCommand(Point startPoint, Point endPoint, List<FloorTile> modifiedFloorTileBefore, List<FloorTile> modifiedFloorTileAfter)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.modifiedFloorTileBefore = modifiedFloorTileBefore;
            this.modifiedFloorTileAfter = modifiedFloorTileAfter;
        }
        public Point GetStartPoint()
        {
            return startPoint;
        }
        public Point GetEndPoint()
        {
            return endPoint;
        }
        public List<FloorTile> GetModifiedFloorTileBefore()
        {
            return modifiedFloorTileBefore;
        }
        public List<FloorTile> GetModifiedFloorTileAfter()
        {
            return modifiedFloorTileAfter;
        }
    }
}
