using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FerrumModules.Engine
{
    public abstract class Sprite : Entity
    {
        public Texture2D Texture;
        private Rectangle sourceRect = new Rectangle();

        private int _tileWidth;
        public int TileWidth
        {
            get { return _tileWidth; }
            set
            {
                _tileWidth = value;
                sourceRect.Width = _tileWidth;
            }
        }

        private int _tileHeight;
        public int TileHeight
        {
            get { return _tileHeight; }
            set
            {
                _tileHeight = value;
                sourceRect.Height = _tileHeight;
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
                sourceRect.X = _currentFrame % framesPerRow * TileWidth;
                sourceRect.Y = _currentFrame / framesPerRow * TileHeight;
            }
        }

        public bool FlipX = false;
        public bool FlipY = false;
        private SpriteEffects FlipEffects
        {
            get
            {
                var spriteEffects = SpriteEffects.None;
                if (FlipX) spriteEffects |= SpriteEffects.FlipHorizontally;
                if (FlipY) spriteEffects |= SpriteEffects.FlipVertically;
                return spriteEffects;
            }
        }

        public bool Rotating = true;

        public Rectangle BoundingBox
        {
            get
            {
                var boxSize = new Vector2(TileWidth, TileHeight) * GlobalScale;

                var rotatedRect = Rotation.RotatedRectAABB(
                    boxSize,
                    Centered ? Vector2.Zero : boxSize / 2, 
                    GlobalPosition,
                    GlobalAngle);

                return rotatedRect;
            }
        }

        public Sprite() { }

        public Sprite(int tileWidth, int tileHeight)
        {
            TileWidth = tileWidth;
            TileHeight = tileHeight;
        }

        public Sprite(Texture2D loadTexture, int tileWidth, int tileHeight)
        {
            Texture = loadTexture;

            TileWidth = tileWidth;
            TileHeight = tileHeight;
        }

        public override void Render(SpriteBatch spriteBatch)
        {
            base.Render(spriteBatch);

            bool isOnScreen = Collision.RectsCollide(BoundingBox, Scene.Camera.BoundingBox);

            if (Texture != null && isOnScreen)
            {
                var renderTransform = GetRenderTransform();
                Vector2 renderOrigin = Centered ? (new Vector2(TileWidth, TileHeight) / GlobalScale) / 2 * GlobalScale : Vector2.Zero;
                spriteBatch.Draw(
                    Texture,
                    renderTransform.Position,
                    sourceRect,
                    GlobalColor * ((float)GlobalColor.A / 256),
                    Rotating ? renderTransform.Angle : -Scene.Camera.AngleOffset,
                    renderOrigin,
                    renderTransform.Scale,
                    FlipEffects,
                    0.0f);
            }
        }
    }
}
