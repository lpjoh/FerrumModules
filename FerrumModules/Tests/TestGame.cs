using System;

using Crossfrog.Ferrum.Engine;
using Crossfrog.Ferrum.Engine.Entities;
using Crossfrog.Ferrum.Engine.Physics;
using Crossfrog.Ferrum.Engine.Managers;
using Crossfrog.Ferrum.Engine.Modules;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Crossfrog.Ferrum.Tests
{
    public class TestGame : FE_Engine
    {
        public TestGame() : base(640, 360, 1, 1, null, "Ferrum Mario Test") { }

        public enum RenderLayers { BackgroundLayer, TileLayer, PlayerLayer, EnemyLayer, CollisionLayer }

        public override void InitGame()
        {
            base.InitGame();
            FPS = 60;

            var testTimer = CurrentScene.AddManager(new Timer(1, true, true));
            testTimer.Timeout += TestPrint;
            testTimer.Name = "Timer";

            var idleFrames = new int[] { 3, 2, 1 };
            var idleAnim = new SpriteAnimation("idle", 3, 0, idleFrames);

            var runFrames = new int[] { 6, 5, 4 };
            var runAnim = new SpriteAnimation("run", 3, 1, runFrames);

            var runPostFrames = new int[] { 7 };
            var runPostAnim = new SpriteAnimation("runPost", 3, 0, runPostFrames);

            CurrentScene.BackgroundColor = Color.Goldenrod;

            var mario = new AnimatedSprite("mario", 16, 16, idleAnim, runAnim, runPostAnim);
            CurrentScene.AddChild(mario);
            mario.Name = "Mario";
            mario.SetRenderLayer(RenderLayers.PlayerLayer);
            //mario.Visible = false;

            var mario2 = mario.AddChild(new StaticSprite("mario", 16, 16, 8));
            mario2.Name = "Koopa";
            var mario3 = mario2.AddChild(new StaticSprite("mario", 16, 16, 5));
            mario2.SetRenderLayer(RenderLayers.EnemyLayer);
            mario.FlipX = true;
            mario2.Rotating = false;
            //mario.Centered = false;

            var tileScene = CurrentScene.AddChildren(TileMap.LoadSceneFromFile("mixed", RenderLayers.TileLayer).GetEntities());
            CurrentScene["Punk"].PositionOffset = Vector2.Zero;

            var testOffsetter = CurrentScene.AddChild(new Entity());
            testOffsetter.Name = "Offset";
            testOffsetter.PositionOffset.X = 16;
            testOffsetter.ScaleOffset *= 0.5f;

            var punk = CurrentScene["Punk"];
            var collisionBody = testOffsetter.AddChild(new KinematicHitbox());
            collisionBody.AddChild(punk);
            collisionBody.Name = "Player";
            collisionBody.ScaleOffset *= 2;
            //collisionBody.Bounceback = new Vector2(0.8f, 0.8f);

            var shape = collisionBody.AddChild(new HitboxShape(16, 16));
            //shape.ScaleOffset /= 2;
            //var shapedos = collisionBody.AddChild(new HitboxShape(16, 16));
            //shapedos.PositionOffset.X = 8;
            //shapedos.PositionOffset.Y = 8;
            //shape.SetAsRegularShape(8, new Vector2(32, 32));
            //shape.PositionOffset.X = 32;
            //shape.SetAsBox(new Vector2(16, 16));

            var collisionBody2 = CurrentScene.AddChild(new StaticHitbox());
            collisionBody2.Name = "Static";
            //collisionBody2.ScaleOffset *= 3;
            //shape.SetAsBox(new Vector2(16, 16));
            var shape2 = collisionBody2.AddChild(new HitboxShape(64, 64));
            //shape2.SetAsBox(16);
            shape2.PositionOffset.X = 64;
            shape2.PositionOffset.Y = 16;
            var shape3 = collisionBody2.AddChild(new HitboxShape(64, 64));
            //shape3.SetAsBox(new Vector2(16, 16));

            var shape5 = collisionBody2.AddChild(new HitboxShape(64, 64));
            //shape2.SetAsBox(16);
            shape5.PositionOffset.X = 128;
            shape5.PositionOffset.Y = -16;

            var shape4 = CurrentScene.AddChild(new KinematicHitbox());
            shape4.AddChild(new HitboxShape(32, 32));
            shape4.PositionOffset = new Vector2(-64, -32);
            shape4.Velocity = new Vector2(-0.25f, 0);

            var movingShape = shape4.AddChild(new HitboxShape(32, 32));
            movingShape.PositionOffset = new Vector2(-64, -32);

            //CurrentScene["Punk"].Visible = false;

            var testAnim = new Animation<Vector2>("test", true,
                new Keyframe<Vector2>(new Vector2(0, 0), 1f, Interpolation.Cosine),
                new Keyframe<Vector2>(new Vector2(55, 64), 1f),
                new Keyframe<Vector2>(new Vector2(32, 53), 1f),
                new Keyframe<Vector2>(new Vector2(32, 15), 1f),
                new Keyframe<Vector2>(new Vector2(61, 32), 1f));
            var testAnim2 = new Animation<Vector2>("test2", true,
                new Keyframe<Vector2>(new Vector2(0, 0), 0.5f, Interpolation.Linear),
                new Keyframe<Vector2>(new Vector2(128, 32), 0.5f),
                new Keyframe<Vector2>(new Vector2(-48, 64), 0.5f));

            var testPlayer = CurrentScene.AddManager(new PropertyAnimator<Vector2>(testAnim, testAnim2));
            testPlayer.Name = "AnimPlayer";
            testPlayer.PlayAnimation("test");

            var testTileSet2 = CurrentScene.AddChild(new TileMap("big"));
            testTileSet2.SetRenderLayer(RenderLayers.BackgroundLayer);
            testTileSet2.Infinite = true;
            testTileSet2.ScaleOffset = new Vector2(1f, 1f);

            testTileSet2.Visible = false;

            var testCamera = mario.AddChild(new Camera());
            testCamera.Name = "Camera";
            //testCamera.Centered = false;
            CurrentScene.Camera = testCamera;
            collisionBody.AddChild(testCamera);
            testCamera.Zoom = 2f;
            mario.PositionOffset = new Vector2(0, 0);
            //mario.ScaleOffset = new Vector2(2, 2);
            //testCamera.PositionOffset.X = 40;

            mario2.PositionOffset = new Vector2(16, 0);
            mario3.PositionOffset = new Vector2(0, 16);

            mario2.ColorOffset = new Color(Color.White, 0.5f);
            //testTileSet2.Visible = false;

            CurrentScene["Tile Layer 1"].Visible = false;
            CurrentScene["Tile Layer 2"].Visible = false;

            Input.SetAction("move_left", Keys.Left, Buttons.LeftThumbstickLeft);
            Input.SetAction("move_right", Keys.Right, Buttons.LeftThumbstickRight);
            Input.SetAction("move_up", Keys.Up, Buttons.LeftThumbstickUp);
            Input.SetAction("move_down", Keys.Down, Buttons.LeftThumbstickDown);
            Input.SetAction("fire", Keys.Space, Buttons.A);
            Input.SetAction("ShowPhysics", Keys.F, Buttons.B);
        }

        private Vector2 prevVel = Vector2.Zero;
        public override void UpdateGame(float delta)
        {
            base.UpdateGame(delta);
            var player = CurrentScene["Offset"]["Player"] as KinematicHitbox;
            //Console.WriteLine(player.OnFloor);
            prevVel = player.PositionOffset;
            //player.AngleOffset += MathHelper.Pi / 160;
            player.Velocity.Y += 0.25f;

            var speed = 0.1f;
            var decel = 0.98f;
            Console.WriteLine(player.Velocity.X);

            if (Input.ActionPressed("move_right"))
                player.Velocity.X += speed;
            else if (Input.ActionPressed("move_left"))
                player.Velocity.X += -speed;
            else player.Velocity.X *= decel;

            var animPlayer = CurrentScene.GetManager<PropertyAnimator<Vector2>>("AnimPlayer");

            if (Input.ActionJustPressed("fire")) player.Velocity.Y = -5f;
            //animPlayer.Set(ref CurrentScene["Mario"].PositionOffset);
        }
        public void TestPrint()
        {
            //Console.WriteLine("This timer loops and autostarts!");
        }
    }
}
