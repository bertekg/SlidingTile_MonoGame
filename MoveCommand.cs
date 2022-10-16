using Microsoft.Xna.Framework;

namespace SlidingTile_MonoGame
{
    internal class MoveCommand
    {
        private Point startPoint;
        private Point endPoint;
        public MoveCommand(Point startPoint, Point endPoint)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }
        public Point GetStartPoint()
        {
            return startPoint;
        }
        public Point GetEndPoint()
        {
            return endPoint;
        }
    }
}
