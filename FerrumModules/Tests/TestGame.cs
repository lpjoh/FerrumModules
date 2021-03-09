using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using FerrumModules.Engine;

namespace FerrumModules.Tests
{
    public class TestGame : FerrumEngine
    {
        public TestGame() : base(640, 360) { }

        public enum RenderLayers { BackgroundLayer, TileLayer, PlayerLayer, EnemyLayer }

        public override void InitGame()
        {
            base.InitGame();

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);

            var testTimer = CurrentScene.AddManager(new Timer(1, true, true));
            testTimer.Timeout += TestPrint;

            var idleFrames = new List<int>() { 3, 2, 1 };
            var idleAnim = new SpriteAnimation("idle", idleFrames, 3);

            var runFrames = new List<int>() { 6, 5, 4 };
            var runAnim = new SpriteAnimation("run", runFrames, 3, 1);

            var runPostFrames = new List<int>() { 7 };
            var runPostAnim = new SpriteAnimation("runPost", runPostFrames, 3);

            CurrentScene.BackgroundColor = Color.Goldenrod;

            var marioTexture = Assets.Textures["mario"];

            var mario = CurrentScene.AddChild(new AnimatedSprite(marioTexture, 16, 16, idleAnim));
            mario.AddAnimation(runAnim);
            mario.AddAnimation(runPostAnim);
            mario.SetRenderLayer(RenderLayers.PlayerLayer);

            var mario2 = mario.AddChild(new StaticSprite(marioTexture, 16, 16, 8));
            var mario3 = mario2.AddChild(new StaticSprite(marioTexture, 16, 16, 5));
            mario2.SetRenderLayer(RenderLayers.EnemyLayer);

            mario.Name = "Mario";
            mario2.Name = "Koopa";
            //mario.Centered = false;

            var testTileSet = CurrentScene.AddChild(new TileMap("infinite"));
            testTileSet.SetRenderLayer(RenderLayers.TileLayer);
            testTileSet.Name = "TileMap";
            testTileSet.PositionOffset.X = 64;
            testTileSet.Infinite = true;
            //testTileSet.AngleOffset = Rotation.PI / 8;

            var testTileSet2 = CurrentScene.AddChild(new TileMap("big"));
            testTileSet2.SetRenderLayer(RenderLayers.BackgroundLayer);
            testTileSet2.Infinite = true;
            testTileSet2.ScaleOffset = new Vector2(0.5f, 0.5f);

            var testCamera = mario.AddChild(new Camera());
            //testCamera.Centered = false;
            CurrentScene.Camera = testCamera;
            testCamera.Zoom = 2f;
            mario.PositionOffset = new Vector2(0, 0);
            //mario.ScaleOffset = new Vector2(3, 2);
            //testCamera.AngleOffset = Rotation.PI / 8;
            testCamera.PositionOffset.X = 40;

            testTileSet.ParallaxFactorOffset.X = 0.5f; testTileSet.ParallaxFactorOffset.Y = 0.5f;
            testTileSet2.ParallaxFactorOffset.X = 0.25f; testTileSet2.ParallaxFactorOffset.Y = 0.25f;

            testTileSet2.ColorOffset = new Color(Color.White, 0.5f);

            mario.Visible = true;

            mario2.PositionOffset = new Vector2(16, 0);
            mario3.PositionOffset = new Vector2(0, 16);
            //mario2.ScaleOffset = new Vector2(0.5f, 0.5f);

            mario2.ColorOffset = new Color(Color.White, 0.5f);

            Input.SetAction("move_left", Keys.Left, Buttons.LeftThumbstickLeft);
            Input.SetAction("move_right", Keys.Right, Buttons.LeftThumbstickRight);
            Input.SetAction("move_up", Keys.Up, Buttons.LeftThumbstickUp);
            Input.SetAction("move_down", Keys.Down, Buttons.LeftThumbstickDown);
            Input.SetAction("fire", Keys.Space, Buttons.A);
        }

        public override void UpdateGame(float delta)
        {
            base.UpdateGame(delta);
            var player = CurrentScene["Mario"] as AnimatedSprite;

            //Console.WriteLine(player.PositionOffset);

            //Console.WriteLine(CurrentScene.Camera.BoundingBox);
            //Console.WriteLine(player.GlobalPosition);

            //CurrentScene.Camera.AngleOffset += Rotation.PI / 600;
            //CurrentScene["Mario"].AngleOffset += Rotation.PI * delta;
            //CurrentScene["Mario"].ScaleOffset.Y += 0.01f;
            //CurrentScene.Camera.Zoom += 0.01f;

            var tileMap = CurrentScene["TileMap"] as TileMap;
            //tileMap.PositionOffset.X += 0.1f; tileMap.PositionOffset.Y += 0.1f;
            //Console.WriteLine(tileMap.PositionOffset.X);

            //tileMap.ParallaxFactorOffset.X -= 0.001f; tileMap.ParallaxFactorOffset.Y -= 0.001f;

            var speed = 1;

            if (Input.ActionJustPressed("move_left") || Input.ActionJustPressed("move_right"))
                player.PlayAnimation("run");
            if (Input.ActionJustReleased("move_left") || Input.ActionJustReleased("move_right"))
                player.PlayAnimation("idle");

            if (Input.ActionJustPressed("fire")) tileMap.InfiniteY = !tileMap.InfiniteY;

            if (Input.ActionPressed("move_right"))
                player.PositionOffset.X += speed;
            else if (Input.ActionPressed("move_left"))
                player.PositionOffset.X -= speed;

            if (Input.ActionPressed("move_down"))
                player.PositionOffset.Y += speed;
            else if (Input.ActionPressed("move_up"))
                player.PositionOffset.Y -= speed;

            //if (Input.IsActionPressed("move_up")) player.AngleOffset += Rotation.PI / 16;
            //if (Input.IsActionJustPressed("move_up"))
            //    player.Velocity = new Vector2(player.Velocity.X, -100);
        }
        public void TestPrint()
        {
            //Console.WriteLine("This timer loops and autostarts!");
        }
    }
}
