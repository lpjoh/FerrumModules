using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using FerrumModules.Engine;

namespace FerrumModules.Tests
{
    public class TestGame : FE_Engine
    {
        public TestGame() : base(640, 240) { }

        public enum RenderLayers { TileLayer, EnemyLayer, PlayerLayer }

        public override void InitGame()
        {
            base.InitGame();
            var marioFrames = new List<int>() { 0, 1, 2, 3, 4 };
            var marioAnim = new FE_Animation(marioFrames, 6);

            var marioTexture = FE_Assets.Textures["mario"];
            var mario = CurrentScene.Add(new FE_AnimatedSprite(marioTexture, 16, 16, marioAnim));
            mario.SetRenderLayer(RenderLayers.PlayerLayer);
            var mario2 = CurrentScene.Add(new FE_StaticSprite(marioTexture, 16, 16, 8));
            mario2.SetRenderLayer(RenderLayers.EnemyLayer);

            var testTileSet = CurrentScene.Add(new FE_TileMap("big"));
            testTileSet.SetRenderLayer(RenderLayers.TileLayer);

            var marioPhys = CurrentScene.Add(new FE_PhysicsEntity(mario));

            var mario2Phys = CurrentScene.Add(new FE_PhysicsEntity(mario2));

            mario.Name = "Mario";
            marioPhys.Name = "MarioPhys";
            mario2.Name = "Koopa";

            var testCamera = CurrentScene.Add(new FE_Camera());
            testCamera.PivotEntity = mario;
            CurrentScene.Camera = testCamera;
            testCamera.Scale.X = 2;
            testCamera.Scale.Y = 2;

            marioPhys.Velocity = new Vector2(50, 25);

            marioPhys.Position = new Vector2(-64, -64);
            mario2Phys.Position = new Vector2(0, -32);

            Console.WriteLine(CurrentScene.PhysicsWorld.BodyCount);

            FE_Input.AddAction("move_left", Keys.Left, Buttons.LeftThumbstickLeft);
            FE_Input.AddAction("move_right", Keys.Right, Buttons.LeftThumbstickRight);
            FE_Input.AddAction("move_up", Keys.Up, Buttons.LeftThumbstickUp);
            FE_Input.AddAction("move_down", Keys.Down, Buttons.LeftThumbstickDown);
        }

        public override void UpdateGame(float delta)
        {
            base.UpdateGame(delta);
            var player = CurrentScene.Get<FE_TransformEntity>("Mario");
            if (FE_Input.IsActionPressed("move_right"))
                player.Position.X += 1f;
            else if (FE_Input.IsActionPressed("move_left"))
                player.Position.X -= 1f;

            if (FE_Input.IsActionPressed("move_up"))
                player.Position.Y -= 1f;
            else if (FE_Input.IsActionPressed("move_down"))
                player.Position.Y += 1f;
        }
    }
}
