using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FerrumModules.Engine
{
    public abstract class FE_Sprite : FE_TransformEntity
    {
        public Texture2D Texture;
        private Rectangle srcRect = new Rectangle();

        private int _tileWidth;
        public int TileWidth
        {
            get { return _tileWidth; }
            set
            {
                _tileWidth = value;
                srcRect.Width = _tileWidth;
            }
        }

        private int _tileHeight;
        public int TileHeight
        {
            get { return _tileHeight; }
            set
            {
                _tileHeight = value;
                srcRect.Height = _tileHeight;
            }
        }

        private int _currentFrame;
        public int CurrentFrame
        {
            get { return _currentFrame; }
            set
            {
                _currentFrame = value;
                int framesPerRow = (Texture.Width / TileWidth);
                srcRect.X = _currentFrame % framesPerRow * TileWidth;
                srcRect.Y = _currentFrame / framesPerRow * TileHeight;
            }
        }

        public FE_Sprite(Texture2D loadTexture, int tileWidth, int tileHeight)
        {
            Texture = loadTexture;

            Position = new Vector2(0.0f, 0.0f);
            Scale = new Vector2(1.0f, 1.0f);

            TileWidth = tileWidth;
            TileHeight = tileHeight;
        }

        public Rectangle GetBoundingBox()
        {
            var boundingBox = new Rectangle(
                    (int)Position.X,
                    (int)Position.Y,
                    (int)(TileWidth * Scale.X),
                    (int)(TileWidth * Scale.Y)
                );

            if (Centered)
            {
                boundingBox.X -= TileWidth / 2;
                boundingBox.Y -= TileWidth / 2;
            }

            return boundingBox;
        }

        public override void Update(float delta)
        {
            base.Update(delta);
        }

        public override void Render(SpriteBatch spriteBatch, SpriteEffects spriteBatchEffects)
        {
            base.Render(spriteBatch, spriteBatchEffects);

            bool isOnScreen = FE_Collision.RectsCollide(Scene.Camera.BoundingBox, GetBoundingBox());

            if (Texture != null && isOnScreen)
            {
                Vector2 renderOrigin = Centered ? new Vector2(TileWidth, TileHeight) / 2 : Vector2.Zero;
                spriteBatch.Draw(
                    Texture,
                    RenderPosition,
                    srcRect,
                    Color.White,
                    0.0f,
                    renderOrigin,
                    RenderScale,
                    spriteBatchEffects,
                    0.0f);
            }
        }
    }
}
