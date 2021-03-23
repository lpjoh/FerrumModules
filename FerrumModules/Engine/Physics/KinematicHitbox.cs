using System;
using Microsoft.Xna.Framework;
using Crossfrog.Ferrum.Engine.Modules;

namespace Crossfrog.Ferrum.Engine.Physics
{
    public class KinematicHitbox : StaticHitbox
    {
        public const int Iterations = 32;
        public Vector2 Velocity;

        private Vector2 MoverExtents;
        private Vector2 MoverPosition;
        private Vector2 MoverSize;
        private Vector2 MoverEndPosition;

        private Vector2 ColliderExtents;
        private Vector2 ColliderPosition;
        private Vector2 ColliderSize;
        private Vector2 ColliderEndPosition;

        private Vector2 MoverColliderDifferenceWindow;

        private bool MoverColliderIntersect()
        {
            return Collision.RectsCollide(MoverPosition, MoverSize, ColliderPosition, ColliderSize);
        }
        private void UpdateDifferenceWindow()
        {
            var windowX = Collision.DifferenceWindow(MoverPosition.X, MoverEndPosition.X, ColliderPosition.X, ColliderEndPosition.X);
            var windowY = Collision.DifferenceWindow(MoverPosition.Y, MoverEndPosition.Y, ColliderPosition.Y, ColliderEndPosition.Y);
            MoverColliderDifferenceWindow = new Vector2(windowX, windowY);
        }
        private void UpdateMoverPosition(HitboxShape mover)
        {
            MoverPosition = mover.GlobalPosition - MoverExtents;
            MoverEndPosition = MoverPosition + MoverSize;
        }
        public void ResolveFor(HitboxShape mover, HitboxShape collider)
        {
            MoverExtents = mover.GlobalExtents;
            MoverPosition = mover.GlobalPosition - MoverExtents;
            MoverSize = mover.BoxSize * mover.GlobalScale;
            MoverEndPosition = MoverPosition + MoverSize;

            ColliderExtents = collider.GlobalExtents;
            ColliderPosition = collider.GlobalPosition - ColliderExtents;
            ColliderSize = collider.BoxSize * collider.GlobalScale;
            ColliderEndPosition = ColliderPosition + ColliderSize;

            if (MoverColliderIntersect())
            {
                UpdateDifferenceWindow();
                if (Math.Abs(MoverColliderDifferenceWindow.X) < Math.Abs(MoverColliderDifferenceWindow.Y))
                {
                    ResolveX();
                    UpdateMoverPosition(mover);
                    if (MoverColliderIntersect())
                        ResolveY();
                }
                else if (Math.Abs(MoverColliderDifferenceWindow.X) > Math.Abs(MoverColliderDifferenceWindow.Y))
                {
                    ResolveY();
                    UpdateMoverPosition(mover);
                    if (MoverColliderIntersect())
                        ResolveX();
                }
            }
        }
        public override void Update(float delta)
        {
            for (int i = 0; i < Iterations; i++)
            {
                PositionOffset += Velocity / Iterations;
                foreach (var body in Scene.PhysicsWorld)
                {
                    if (body.Parent == this) continue;

                    var bodyType = body.Parent?.GetType();
                    if (typeof(StaticHitbox).IsAssignableFrom(bodyType))
                    {
                        var collider = body.Parent as StaticHitbox;
                        foreach (var cBox in collider.Hitboxes)
                            foreach (var mBox in Hitboxes)
                            {
                                ResolveFor(mBox, cBox);

                                UpdateMoverPosition(mBox);
                                if (MoverColliderIntersect())
                                {
                                    UpdateDifferenceWindow();

                                    PositionOffset += MoverColliderDifferenceWindow;
                                }
                            }
                    }
                }
            }

            base.Update(delta);
        }

        private void ResolveX()
        {
            Collision.Resolve1D(
                    MoverPosition.X,
                    MoverEndPosition.X,
                    ColliderPosition.X,
                    ColliderEndPosition.X,
                    ref Velocity.X,
                    ref PositionOffset.X);
        }
        private void ResolveY()
        {
            Collision.Resolve1D(
                    MoverPosition.Y,
                    MoverEndPosition.Y,
                    ColliderPosition.Y,
                    ColliderEndPosition.Y,
                    ref Velocity.Y,
                    ref PositionOffset.Y);
        }
    }
}