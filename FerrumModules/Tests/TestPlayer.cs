using System;
using System.Diagnostics;

using Crossfrog.Ferrum.Engine;
using Crossfrog.Ferrum.Engine.Modules;
using Crossfrog.Ferrum.Engine.Entities;
using Crossfrog.Ferrum.Engine.Physics;

namespace Crossfrog.Ferrum.Tests
{
    public class TestPlayer : KinematicBody
    {
        public const float Speed = 0.05f;
        public const float JumpHeight = 3.5f;
        public const float Gravity = 0.125f;
        public const float Decel = 0.1f;

        public AnimatedSprite Sprite;
        public CollisionShape Hitbox;
        public Camera Camera;
        public Sensor Area;

        public override void Init()
        {
            Sprite = AddChild(new AnimatedSprite("mario", 16, 16, new SpriteAnimation(new int[] { 0 }, "fuck", 6, 0)));
            Sprite.ScaleOffset *= 0.5f;
            Hitbox = AddChild(new CollisionShape().SetAsBox(8, 10));
            Hitbox.PositionOffset.Y = 1.5f;
            Camera = AddChild(new Camera());
            Scene.Camera = Camera;
            Camera.Zoom = 2;
            //Bounceback = new Vector2(0.5f, 0.5f);
        }
        public override void Update(float delta)
        {
            base.Update(delta);

            var sixtyDelta = delta * 60;

            //Area.AngleOffset += MathHelper.Pi / 160;

            if (!OnFloor)
                Velocity.Y += Gravity;
            if (Input.ActionPressed("move_left"))
            {
                Velocity.X -= Speed;
                Sprite.FlipX = true;
            }
            else if (Input.ActionPressed("move_right"))
            {
                Velocity.X += Speed;
                Sprite.FlipX = false;
            }
            else
            {
                Velocity.X -= Decel * Math.Sign(Velocity.X);

                if (Math.Abs(Velocity.X) <= Decel)
                    Velocity.X = 0;
            }

            if (Input.ActionJustPressed("fire"))
                Velocity.Y = -JumpHeight;
            else if (Input.ActionJustReleased("fire") && Velocity.Y < 0)
                Velocity.Y *= 0.5f;
        }
    }
}
