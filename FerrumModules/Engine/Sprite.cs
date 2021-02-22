using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FerrumModules.Engine
{
    public abstract class Sprite : Entity
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

        public Sprite(Texture2D loadTexture, int tileWidth, int tileHeight)
        {
            Texture = loadTexture;

            PositionOffset = Vector2.Zero;
            ScaleOffset = new Vector2(1.0f, 1.0f);

            TileWidth = tileWidth;
            TileHeight = tileHeight;
        }

        public Rectangle GetBoundingBox()
        {
            var boundingBox = new Rectangle(
                    (int)GlobalPosition.X,
                    (int)GlobalPosition.Y,
                    (int)(TileWidth * ScaleOffset.X),
                    (int)(TileWidth * ScaleOffset.Y)
                );

            if (Centered)
            {
                boundingBox.X -= (int)(TileWidth * ScaleOffset.X) / 2;
                boundingBox.Y -= (int)(TileHeight * ScaleOffset.Y) / 2;
            }

            return boundingBox;
        }

        public override void Render(SpriteBatch spriteBatch, SpriteEffects spriteBatchEffects)
        {
            base.Render(spriteBatch, spriteBatchEffects);

            bool isOnScreen = Collision.RectsCollide(Scene.Camera.BoundingBox, GetBoundingBox());
            isOnScreen = true;

            if (Texture != null && isOnScreen)
            {
                Vector2 renderOrigin = Centered ? (new Vector2(TileWidth, TileHeight) / GlobalScale) / 2 * GlobalScale : Vector2.Zero;
                spriteBatch.Draw(
                    Texture,
                    RenderPosition,
                    srcRect,
                    Color.White,
                    RenderAngle,
                    renderOrigin,
                    RenderScale,
                    spriteBatchEffects,
                    0.0f);
            }
        }
    }
}
