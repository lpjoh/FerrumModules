using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using FerrumModules.Engine;

namespace FerrumModules.Tests
{
    public class TestGame : FerrumContext
    {
        public TestGame() : base(1280, 720) { }

        public enum RenderLayers { TileLayer, EnemyLayer, PlayerLayer }

        public override void InitGame()
        {
            base.InitGame();
            var marioFrames = new List<int>() { 0, 1, 2, 3, 4 };
            var marioAnim = new Animation(marioFrames, 6);

            var marioTexture = Assets.Textures["mario"];

            var testTileSet = CurrentScene.AddChild(new TileMap("big"));
            testTileSet.SetRenderLayer(RenderLayers.TileLayer);

            var marioPhys = CurrentScene.AddChild(new RigidBody());

            var mario2Phys = CurrentScene.AddChild(new RigidBody());

            var mario = marioPhys.AddChild(new AnimatedSprite(marioTexture, 16, 16, marioAnim));
            mario.SetRenderLayer(RenderLayers.PlayerLayer);
            var mario2 = mario2Phys.AddChild(new StaticSprite(marioTexture, 16, 16, 8));
            mario2.SetRenderLayer(RenderLayers.EnemyLayer);

            mario.Name = "Mario";
            marioPhys.Name = "MarioPhys";
            mario2.Name = "Koopa";
            mario2Phys.Name = "KoopaPhys";

            var testCamera = marioPhys.AddChild(new Camera());
            testCamera.ScaleOffset = new Vector2(2, 2);
            CurrentScene.Camera.Centered = true;
            CurrentScene.Camera.PositionOffset = new Vector2(512, 512);

            marioPhys.Velocity = new Vector2(50, 25);
            testCamera.AngleOffset = (float)Math.PI / 4;

            marioPhys.PositionOffset = new Vector2(32, -64);
            mario2Phys.PositionOffset = new Vector2(0, -32);

            Input.AddAction("move_left", Keys.Left, Buttons.LeftThumbstickLeft);
            Input.AddAction("move_right", Keys.Right, Buttons.LeftThumbstickRight);
            Input.AddAction("move_up", Keys.Up, Buttons.LeftThumbstickUp);
            Input.AddAction("move_down", Keys.Down, Buttons.LeftThumbstickDown);
        }

        public override void UpdateGame(float delta)
        {
            base.UpdateGame(delta);
            var player = CurrentScene.GetByName<RigidBody>("MarioPhys");
            if (Input.IsActionPressed("move_right"))
                player.Velocity = new Vector2(player.Velocity.X + 1.0f, player.Velocity.Y);
            else if (Input.IsActionPressed("move_left"))
                player.Velocity = new Vector2(player.Velocity.X - 1.0f, player.Velocity.Y);

            if (Input.IsActionJustPressed("move_up"))
                player.Velocity = new Vector2(player.Velocity.X, -100);
        }
    }
}
