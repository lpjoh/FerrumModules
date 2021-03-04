using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using FerrumModules.Engine;

namespace FerrumModules.Tests
{
    public class TestGame : Engine.Engine
    {
        public TestGame() : base(640, 480) { }

        public enum RenderLayers { TileLayer, EnemyLayer, PlayerLayer }

        public override void InitGame()
        {
            base.InitGame();
            var testManager = CurrentScene.AddManager(new TestManager());
            var marioFrames = new List<int>() { 0, 1, 2, 3, 4 };
            var marioAnim = new Animation(marioFrames, 6);
            
            var marioTexture = Assets.Textures["mario"];

            var mario = CurrentScene.AddChild(new AnimatedSprite(marioTexture, 16, 16, marioAnim));
            mario.SetRenderLayer(RenderLayers.PlayerLayer);
            var mario2 = mario.AddChild(new StaticSprite(marioTexture, 16, 16, 8));
            var mario3 = mario2.AddChild(new StaticSprite(marioTexture, 16, 16, 5));
            mario2.SetRenderLayer(RenderLayers.EnemyLayer);

            mario.Name = "Mario";
            mario2.Name = "Koopa";

            var testTileSet = CurrentScene.AddChild(new TileMap("big"));
            testTileSet.SetRenderLayer(RenderLayers.TileLayer);
            testTileSet.Name = "TileMap";
            //testTileSet.ScaleOffset = new Vector2(1, 3);
            //testTileSet.AngleOffset = Rotation.PI / 8;

            var testCamera = mario.AddChild(new Camera());
            //testCamera.Centered = false;
            CurrentScene.Camera = testCamera;
            testCamera.Zoom = 3f;
            mario.PositionOffset = new Vector2(0, 0);
            testCamera.AngleOffset = Rotation.PI / 4;

            //mario.ScaleOffset = new Vector2(26.6666f, 26.6666f);

            mario2.PositionOffset = new Vector2(16, 0);
            mario3.PositionOffset = new Vector2(0, 16);
            mario2.ScaleOffset = new Vector2(0.5f, 0.5f);

            Input.AddAction("move_left", Keys.Left, Buttons.LeftThumbstickLeft);
            Input.AddAction("move_right", Keys.Right, Buttons.LeftThumbstickRight);
            Input.AddAction("move_up", Keys.Up, Buttons.LeftThumbstickUp);
            Input.AddAction("move_down", Keys.Down, Buttons.LeftThumbstickDown);
        }

        public override void UpdateGame(float delta)
        {
            base.UpdateGame(delta);
            var player = CurrentScene["Mario"];
            
            //CurrentScene.Camera.PositionOffset += new Vector2(0, 0.05f);
            CurrentScene.Camera.AngleOffset += (float)Math.PI / 600;
            //CurrentScene.Camera.Zoom += 0.01f;

            if (Input.IsActionPressed("move_right"))
                player.PositionOffset += new Vector2(1, 0);
            else if (Input.IsActionPressed("move_left"))
                player.PositionOffset -= new Vector2(1, 0);

            if (Input.IsActionPressed("move_up"))
                player.AngleOffset += Rotation.PI / 16;
            //if (Input.IsActionJustPressed("move_up"))
            //    player.Velocity = new Vector2(player.Velocity.X, -100);
        }
    }
}
