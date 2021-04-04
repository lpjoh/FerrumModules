using Microsoft.Xna.Framework;

using Crossfrog.Ferrum.Engine;
using Crossfrog.Ferrum.Engine.Modules;
using Crossfrog.Ferrum.Engine.Entities;
using Crossfrog.Ferrum.Engine.Physics;

namespace Crossfrog.Ferrum.Tests
{
    public class TestPlayer : KinematicHitbox
    {
        public const float Speed = 0.05f;
        public const float JumpHeight = 3.5f;
        public const float Gravity = 0.125f;
        public const float Decel = 0.9f;

        public AnimatedSprite Sprite;
        public HitboxShape Hitbox;
        public Camera Camera;
        public Sensor Area;

        public override void Init()
        {
            ScaleOffset *= 2;
            Sprite = AddChild(new AnimatedSprite("mario", 16, 16, new SpriteAnimation(new int[] { 0 }, "fuck", 6, 0)));
            Sprite.ScaleOffset *= 0.5f;
            Hitbox = AddChild(new HitboxShape(8, 10));
            Hitbox.PositionOffset.Y = -1;
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

            Velocity.Y += Gravity * sixtyDelta;
            if (Input.ActionPressed("move_left"))
            {
                Velocity.X -= Speed * sixtyDelta;
                Sprite.FlipX = true;
            }
            else if (Input.ActionPressed("move_right"))
            {
                Velocity.X += Speed * sixtyDelta;
                Sprite.FlipX = false;
            }
            else
                Velocity.X *= Decel * sixtyDelta;

            if (Input.ActionJustPressed("fire") && OnFloor)
                Velocity.Y = -JumpHeight;
            else if (Input.ActionJustReleased("fire") && Velocity.Y < 0)
                Velocity.Y *= 0.5f;
        }
    }
}
