using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SlidingTile_MonoGame.Class
{
    public class Tile : Sprite
    {
        public FloorTile FloorTile;
        public SpriteFont DigitFont;
        public Tile(Texture2D texture) : base(texture)
        {
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if ((FloorTile.Type == FloorTileType.Normal ||
                FloorTile.Type == FloorTileType.Ice ||
                FloorTile.Type == FloorTileType.Portal ||
                FloorTile.Type == FloorTileType.Spring) &&
                FloorTile.Number == 0) return;
            base.Draw(spriteBatch);
            string textInside = string.Empty;
            Color colorText = new Color();
            Vector2 vectorTextOffset = new Vector2();
            switch (FloorTile.Type)
            {
                case FloorTileType.None:
                    break;
                case FloorTileType.Finish:
                    textInside = "F";
                    colorText = Color.Red;
                    vectorTextOffset = new Vector2(35, 15);
                    break;
                case FloorTileType.Normal:
                    if (FloorTile.PosX != 0 || FloorTile.PosY != 0)
                    {
                        textInside = FloorTile.Number.ToString();
                        vectorTextOffset = new Vector2(35, 15);
                    }
                    else
                    {
                        textInside = $"S{FloorTile.Number}";
                        vectorTextOffset = new Vector2(15, 15);
                    }
                    colorText = Color.Black;
                    break;
                case FloorTileType.Ice:
                    textInside = FloorTile.Number.ToString();
                    colorText = Color.Black;
                    vectorTextOffset = new Vector2(35, 15);
                    break;
                case FloorTileType.Static:
                    break;
                case FloorTileType.Portal:
                    textInside = FloorTile.Portal.ToString();
                    colorText = Color.Black;
                    vectorTextOffset = new Vector2(35, 15);
                    break;
                case FloorTileType.Spring:
                    textInside = FloorTile.Number.ToString();
                    colorText = Color.Black;
                    vectorTextOffset = new Vector2(50, 15);
                    break;
                    break;
                default:
                    break;
            }
            spriteBatch.DrawString(DigitFont, textInside, Position + vectorTextOffset, colorText);
        }
    }
}
