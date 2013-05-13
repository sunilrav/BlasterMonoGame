using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blaster
{
    class GameObject
    {
        public Vector2 Position;
        public Texture2D Texture;

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.White);
        }
    }
}
