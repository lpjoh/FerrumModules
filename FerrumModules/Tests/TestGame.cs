using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FerrumModules.Engine;

namespace FerrumModules.Tests
{
    public class TestGame : FE_Engine
    {
        public Texture2D marioTexture;
        public Texture2D pixFont;
        public override void LoadGameContent()
        {
            base.LoadGameContent();
            marioTexture = Content.Load<Texture2D>("mario");
            pixFont = Content.Load<Texture2D>("pixfont");
        }

        public override void InitGame()
        {
            base.InitGame();
            var marioFrames = new List<int>() { 0, 1, 2, 3, 4 };
            var marioAnim = new FE_Animation(marioFrames, 6);

            var testTileSet = CurrentScene.Add(new FE_TileMap("big.tmx", pixFont));
            var mario = CurrentScene.Add(new FE_AnimatedSprite(marioTexture, 16, 16, marioAnim));
            var mario2 = CurrentScene.Add(new FE_StaticSprite(marioTexture, 16, 16, 8));

            mario.Name = "Mario";
            mario2.Name = "Koopa";

            var testCamera = CurrentScene.Add(new FE_Camera());
            testCamera.Scale = new Vector2(4, 4);
            testCamera.PivotEntity = mario;
            CurrentScene.Camera = testCamera;

            mario2.Position = new Vector2(32, 32);
            Console.WriteLine(CurrentScene.EntityList.Count);
        }

        public override void UpdateGame(float delta)
        {
            CurrentScene.Get<FE_TransformEntity>("Mario").Position.X += 2f;
            CurrentScene.Get<FE_TransformEntity>("Mario").Position.Y += 2f;
            base.UpdateGame(delta);
        }
    }
}
