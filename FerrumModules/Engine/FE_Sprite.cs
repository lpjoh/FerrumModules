using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FerrumModules.Engine
{
    public abstract class FE_Sprite : FE_TransformEntity
    {
        public Texture2D Texture;
        public bool Centered = true;
        private Rectangle srcRect = new Rectangle();

        private int _tileSizeX;
        public int TileSizeX
        {
            get { return _tileSizeX; }
            set
            {
                _tileSizeX = value;
                srcRect.Width = _tileSizeX;
            }
        }

        private int _tileSizeY;
        public int TileSizeY
        {
            get { return _tileSizeY; }
            set
            {
                _tileSizeY = value;
                srcRect.Height = _tileSizeY;
            }
        }

        private int _currentFrame;
        public int CurrentFrame
        {
            get { return _currentFrame; }
            set
            {
                _currentFrame = value;
                int framesPerRow = (Texture.Width / TileSizeX);
                srcRect.X = _currentFrame % framesPerRow * TileSizeX;
                srcRect.Y = _currentFrame / framesPerRow * TileSizeY;
            }
        }

        public FE_Sprite(Texture2D loadTexture, int tileSizeX, int tileSizeY)
        {
            Texture = loadTexture;
            if (Texture == null) throw new Exception("Sprite texture did not load correctly.");

            Position = new Vector2(0.0f, 0.0f);
            Scale = new Vector2(1.0f, 1.0f);

            TileSizeX = tileSizeX;
            TileSizeY = tileSizeY;
        }

        public override void Update(float delta)
        {
            base.Update(delta);
        }

        public override void Render(SpriteBatch spriteBatch, SpriteEffects spriteBatchEffects)
        {
            base.Render(spriteBatch, spriteBatchEffects);

            var boundingBox =
                new Rectangle (
                    (int)Position.X,
                    (int)Position.Y,
                    (int)(TileSizeX * Scale.X),
                    (int)(TileSizeX * Scale.Y)
                );

            bool isOnScreen = FE_Collision.RectsCollide(Scene.Camera.BoundingBox, boundingBox);

            if (Texture != null && isOnScreen)
            {
                Vector2 renderOrigin = Centered ? new Vector2(TileSizeX, TileSizeY) / 2 : Vector2.Zero;
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
