namespace SlidingTile_MonoGame
{
    public enum FloorTileType { None, Finish, Normal, Ice };
    public class FloorTile
    {
        public int PosX { get; set; }
        public int PosY { get; set; }
        public FloorTileType Type { get; set; }
        public int Number { get; set; }
    }
}
