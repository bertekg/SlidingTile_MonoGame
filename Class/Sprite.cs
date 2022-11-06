using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SlidingTile_MonoGame.Class
{
    public class Sprite
    {
        private Texture2D _texture;
        public Vector2 Position;
        public Sprite(Texture2D texture)
        {
            _texture = texture;
        }
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);
            }
        }
        public virtual void Update(float gameTime)
        {

        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Position, Color.White);
        }
    }
}
