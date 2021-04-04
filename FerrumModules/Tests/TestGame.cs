using System;

using Crossfrog.Ferrum.Engine;
using Crossfrog.Ferrum.Engine.Entities;
using Crossfrog.Ferrum.Engine.Physics;
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

            //var player = CurrentScene.AddChild(new TestPlayer());
            //player.PositionOffset.X = 64;
            //player.PositionOffset.Y = 64;

            var floor = CurrentScene.AddChild(new KinematicHitbox());
            floor.PositionOffset.Y = 64;
            floor.ScaleOffset.X = 6;
            floor.AddChild(new HitboxShape(16, 16));
            floor.AddChild(new StaticSprite("mario", 16, 16));
            floor.Name = "Floor";
            CurrentScene.BackgroundColor = Color.Goldenrod;

            CurrentScene.AddChildren(TileMap.LoadSceneFromFile("funni", RenderLayers.TileLayer).GetEntities());

            var tileSet = CurrentScene["Tile Layer 1"] as TileMap;
            tileSet.CollisionActive = true;
            tileSet.SetCameraBounds();
            //tileSet.ColorOffset = Color.Black;

            Input.SetAction("move_left", Keys.Left, Buttons.LeftThumbstickLeft);
            Input.SetAction("move_right", Keys.Right, Buttons.LeftThumbstickRight);
            Input.SetAction("move_up", Keys.Up, Buttons.LeftThumbstickUp);
            Input.SetAction("move_down", Keys.Down, Buttons.LeftThumbstickDown);
            Input.SetAction("fire", Keys.Space, Buttons.A);
            Input.SetAction("ShowPhysics", Keys.F, Buttons.B);
        }
        public override void UpdateGame(float delta)
        {
            base.UpdateGame(delta);
        }
        public void TestPrint()
        {
            //Console.WriteLine("This timer loops and autostarts!");
        }
    }
}
