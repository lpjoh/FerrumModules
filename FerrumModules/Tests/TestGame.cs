using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FerrumModules.Engine;

namespace FerrumModules.Tests
{
    public class TestGame : FE_Engine
    {
        public TestGame() : base(1280, 720) { }

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
            testCamera.PivotEntity = mario;
            CurrentScene.Camera = testCamera;
            testCamera.Scale.X = 4;
            testCamera.Scale.Y = 4;

            mario2.Position = new Vector2(32, 32);

            FE_Input.AddAction("move_left", Keys.Left, Buttons.LeftThumbstickLeft);
            FE_Input.AddAction("move_right", Keys.Right, Buttons.LeftThumbstickRight);

            Console.WriteLine(CurrentScene.EntityList.Count);
        }

        public override void UpdateGame(float delta)
        {
            if (FE_Input.IsActionJustPressed("move_right"))
                CurrentScene.Get<FE_TransformEntity>("Mario").Position.X += 16f;
            else if (FE_Input.IsActionJustPressed("move_left"))
                CurrentScene.Get<FE_TransformEntity>("Mario").Position.X -= 16f;

            base.UpdateGame(delta);
        }
    }
}
