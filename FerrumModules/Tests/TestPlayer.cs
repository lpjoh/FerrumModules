using Microsoft.Xna.Framework;

using Crossfrog.Ferrum.Engine;
using Crossfrog.Ferrum.Engine.Modules;
using Crossfrog.Ferrum.Engine.Entities;
using Crossfrog.Ferrum.Engine.Physics;

namespace Crossfrog.Ferrum.Tests
{
    public class TestPlayer : KinematicHitbox
    {
        public const float Speed = 0.1f;
        public const float JumpHeight = 6f;
        public const float Gravity = 0.25f;
        public const float Decel = 0.8f;

        public StaticSprite Sprite;
        public HitboxShape Hitbox;
        public Camera Camera;
        public Sensor Area;

        public override void Init()
        {
            ScaleOffset *= 1;
            Sprite = AddChild(new StaticSprite("mario", 16, 16));
            Hitbox = AddChild(new HitboxShape(12, 14));
            Hitbox.PositionOffset.Y = 1;
            Camera = AddChild(new Camera());
            Scene.Camera = Camera;
            Camera.Zoom = 2;
        }
        public override void Update(float delta)
        {
            base.Update(delta);

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
                Velocity.X *= Decel;

            if (Input.ActionJustPressed("fire") && OnFloor)
                Velocity.Y = -JumpHeight;
            if (Input.ActionJustReleased("fire") && !OnFloor && Velocity.Y < 0)
                Velocity.Y *= 0.5f;
        }
    }
}
