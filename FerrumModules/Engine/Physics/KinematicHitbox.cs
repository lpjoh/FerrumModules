using System;
using Microsoft.Xna.Framework;
using Crossfrog.Ferrum.Engine.Modules;

namespace Crossfrog.Ferrum.Engine.Physics
{
    public class KinematicHitbox : HitboxCollider
    {
        public const int Iterations = 20;
        public Vector2 Velocity;

        public override void Update(float delta)
        {
            for (int i = 0; i < Iterations; i++)
            {
                PositionOffset += Velocity / Iterations;
                foreach (var body in Scene.PhysicsWorld)
                {
                if (body.Parent?.GetType() == typeof(StaticHitbox))
                {
                    var collider = body.Parent as StaticHitbox;
                    foreach (var cBox in collider.Hitboxes)
                        foreach (var mBox in Hitboxes)
                        {
                            ResolveFor(mBox, cBox, ref PositionOffset, ref Velocity);

                            var moverPosition = mBox.GlobalPosition - mBox.GlobalExtents;
                            var moverSize = mBox.BoxSize * mBox.GlobalScale;
                            var colliderPosition = cBox.GlobalPosition - cBox.GlobalExtents;
                            var colliderSize = cBox.BoxSize * cBox.GlobalScale;

                            if (Collision.RectsCollide(moverPosition, moverSize, colliderPosition, colliderSize))
                            {
                                var windowX = Collision.DifferenceWindow(moverPosition.X, moverPosition.X + moverSize.X, colliderPosition.X, colliderPosition.X + colliderSize.X);
                                var windowY = Collision.DifferenceWindow(moverPosition.Y, moverPosition.Y + moverSize.Y, colliderPosition.Y, colliderPosition.Y + colliderSize.Y);

                                PositionOffset += new Vector2(windowX, windowY);
                            }
                        }
                    }     
                }
            }

            base.Update(delta);
        }

        private void ResolveX(Vector2 moverPosition, Vector2 moverSize, Vector2 colliderPosition, Vector2 colliderSize, ref Vector2 velocity, ref Vector2 position)
        {
            Collision.Resolve1D(
                    moverPosition.X,
                    moverPosition.X + moverSize.X,
                    colliderPosition.X,
                    colliderPosition.X + colliderSize.X,
                    ref velocity.X,
                    ref position.X);
        }
        private void ResolveY(Vector2 moverPosition, Vector2 moverSize, Vector2 colliderPosition, Vector2 colliderSize, ref Vector2 velocity, ref Vector2 position)
        {
            Collision.Resolve1D(
                    moverPosition.Y,
                    moverPosition.Y + moverSize.Y,
                    colliderPosition.Y,
                    colliderPosition.Y + colliderSize.Y,
                    ref velocity.Y,
                    ref position.Y);
        }
        public void ResolveFor(HitboxShape mover, HitboxShape collider, ref Vector2 position, ref Vector2 velocity)
        {
            var moverExtents = mover.GlobalExtents;
            var moverPosition = mover.GlobalPosition - moverExtents;
            var moverSize = mover.BoxSize * mover.GlobalScale;

            var colliderExtents = collider.GlobalExtents;
            var colliderPosition = collider.GlobalPosition - colliderExtents;
            var colliderSize = collider.BoxSize * collider.GlobalScale;

            if (Collision.RectsCollide(moverPosition, moverSize, colliderPosition, colliderSize))
            {
                var windowX = Collision.DifferenceWindow(moverPosition.X, moverPosition.X + moverSize.X, colliderPosition.X, colliderPosition.X + colliderSize.X);
                var windowY = Collision.DifferenceWindow(moverPosition.Y, moverPosition.Y + moverSize.Y, colliderPosition.Y, colliderPosition.Y + colliderSize.Y);
                if (Math.Abs(windowX) < Math.Abs(windowY))
                {
                    ResolveX(moverPosition, moverSize, colliderPosition, colliderSize, ref velocity, ref position);
                    moverPosition = mover.GlobalPosition - moverExtents;
                    if (Collision.RectsCollide(moverPosition, moverSize, colliderPosition, colliderSize))
                        ResolveY(moverPosition, moverSize, colliderPosition, colliderSize, ref velocity, ref position);
                }
                else if (Math.Abs(windowX) > Math.Abs(windowY))
                {
                    ResolveY(moverPosition, moverSize, colliderPosition, colliderSize, ref velocity, ref position);
                    moverPosition = mover.GlobalPosition - moverExtents;
                    if (Collision.RectsCollide(moverPosition, moverSize, colliderPosition, colliderSize))
                        ResolveX(moverPosition, moverSize, colliderPosition, colliderSize, ref velocity, ref position);
                }
            }
        }
    }
}
