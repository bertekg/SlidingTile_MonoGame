using Microsoft.Xna.Framework.Graphics;
using System.Numerics;

namespace SlidingTile_MonoGame.Class
{
    public class Player : Sprite
    {
        public Player(Texture2D texture, Vector2 intialPosition) : base(texture)
        {
            Position = intialPosition;
        }
    }
}
