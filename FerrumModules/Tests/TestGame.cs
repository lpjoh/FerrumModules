using Crossfrog.FerrumEngine;
using Crossfrog.FerrumEngine.Entities;
using Crossfrog.FerrumEngine.Managers;
using Crossfrog.FerrumEngine.Modules;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FerrumXF.Tests
{
    public class TestGame : FerrumEngine
    {
        public TestGame() : base(640, 360, 1, 1, null, "Ferrum Mario Test") { }

        public enum RenderLayers { BackgroundLayer, TileLayer, PlayerLayer, EnemyLayer, CollisionLayer }

        public override void InitGame()
        {
            base.InitGame();
            FPS = 60;
            CurrentScene.Name = "Scene";

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

            TileMap.ObjectNamespace = GetType().Namespace;
            var tileScene = CurrentScene.AddChildren(TileMap.LoadSceneFromFile("mixed", RenderLayers.TileLayer).GetEntities());
            CurrentScene["Punk"].PositionOffset = Vector2.Zero;

            var collisionBody = CurrentScene.AddChild(new CollisionBody());
            var punk = CurrentScene["Punk"];
            collisionBody.AddChild(punk);
            collisionBody.Name = "Player";
            punk.OpacityOffset = 0.5f;
            collisionBody.SetRenderLayer(RenderLayers.PlayerLayer);
            collisionBody.SetAsBox(16);

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

            var testCamera = mario.AddChild(new Camera());
            testCamera.Name = "Camera";
            //testCamera.Centered = false;
            CurrentScene.Camera = testCamera;
            testCamera.Zoom = 2f;
            mario.PositionOffset = new Vector2(0, 0);
            mario.ScaleOffset = new Vector2(2, 2);
            //testCamera.PositionOffset.X = 40;

            mario2.PositionOffset = new Vector2(16, 0);
            mario3.PositionOffset = new Vector2(0, 16);

            mario2.ColorOffset = new Color(Color.White, 0.5f);
            //testTileSet2.Visible = false;

            Input.SetAction("move_left", Keys.Left, Buttons.LeftThumbstickLeft);
            Input.SetAction("move_right", Keys.Right, Buttons.LeftThumbstickRight);
            Input.SetAction("move_up", Keys.Up, Buttons.LeftThumbstickUp);
            Input.SetAction("move_down", Keys.Down, Buttons.LeftThumbstickDown);
            Input.SetAction("fire", Keys.Space, Buttons.A);
        }

        public override void UpdateGame(float delta)
        {
            base.UpdateGame(delta);
            var player = CurrentScene["Player"];

            //foreach (var v in (player["Poly"] as CollisionBody).GlobalVertices) Console.WriteLine(v);
            //Console.WriteLine("--------");

            var speed = 1f;

            if (Input.ActionPressed("move_right"))
            player.PositionOffset.X += speed;
            else if (Input.ActionPressed("move_left"))
            player.PositionOffset.X -= speed;

            if (Input.ActionPressed("move_down"))
            player.PositionOffset.Y += speed;
            else if (Input.ActionPressed("move_up"))
            player.PositionOffset.Y -= speed;

            var animPlayer = CurrentScene.GetManager<PropertyAnimator<Vector2>>("AnimPlayer");

            if (Input.ActionJustPressed("fire")) player.AddChild(CurrentScene.Camera);

            //animPlayer.Set(ref CurrentScene["Mario"].PositionOffset);
        }
        public void TestPrint()
        {
            //Console.WriteLine("This timer loops and autostarts!");
        }
    }
}
