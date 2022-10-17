using Microsoft.Xna.Framework;
using System.Collections.Generic;
using static SlidingTile_MonoGame.Game1;

namespace SlidingTile_MonoGame
{
    internal class MoveCommand
    {
        private Point startPoint;
        private Point endPoint;
        private List<Cell> modifiedCellsBefore;
        private List<Cell> modifiedCellsAfter;
        public MoveCommand(Point startPoint, Point endPoint, List<Cell> modifiedCellsBefore, List<Cell> modifiedCellsAfter)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.modifiedCellsBefore = modifiedCellsBefore;
            this.modifiedCellsAfter = modifiedCellsAfter;
        }
        public Point GetStartPoint()
        {
            return startPoint;
        }
        public Point GetEndPoint()
        {
            return endPoint;
        }
        public List<Cell> GetModifiedCellsBefore()
        {
            return modifiedCellsBefore;
        }
        public List<Cell> GetModifiedCellsAfter()
        {
            return modifiedCellsAfter;
        }
    }
}
