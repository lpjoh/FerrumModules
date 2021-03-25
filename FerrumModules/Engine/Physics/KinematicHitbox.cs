using System;
using Microsoft.Xna.Framework;
using Crossfrog.Ferrum.Engine.Modules;

namespace Crossfrog.Ferrum.Engine.Physics
{
    public class KinematicHitbox : StaticHitbox
    {
        public const int Iterations = 8;
        public Vector2 Velocity;
        public Vector2 Bounceback = Vector2.Zero;

        private Vector2 MoverExtents;
        private Vector2 MoverPosition;
        private Vector2 MoverSize;
        private Vector2 MoverEndPosition;

        private Vector2 ColliderExtents;
        private Vector2 ColliderPosition;
        private Vector2 ColliderSize;
        private Vector2 ColliderEndPosition;

        public bool OnFloor { get; private set; }
        public bool OnCeiling { get; private set; }
        public bool OnLeftWall { get; private set; }
        public bool OnRightWall { get; private set; }
        public bool OnWall => OnLeftWall || OnRightWall;

        private Vector2 MoverColliderDifferenceWindow;
        private Vector2 ParentScale;

        private bool MoverColliderIntersect(bool includeGrazing = false)
        {
            return Collision.RectsCollide(MoverPosition, MoverSize, ColliderPosition, ColliderSize, includeGrazing);
        }
        private void UpdateDifferenceWindow()
        {
            var windowX = Collision.DifferenceWindow(MoverPosition.X, MoverEndPosition.X, ColliderPosition.X, ColliderEndPosition.X, ParentScale.X);
            var windowY = Collision.DifferenceWindow(MoverPosition.Y, MoverEndPosition.Y, ColliderPosition.Y, ColliderEndPosition.Y, ParentScale.Y);

            MoverColliderDifferenceWindow = new Vector2(windowX, windowY);
        }
        private void CheckCollisionBools()
        {
            if (MoverColliderIntersect(true))
            {
                OnFloor |= Math.Round(MoverEndPosition.Y) == Math.Round(ColliderPosition.Y);
                OnCeiling |= Math.Round(MoverPosition.Y) == Math.Round(ColliderEndPosition.Y);
                OnLeftWall |= Math.Round(MoverPosition.X) == Math.Round(ColliderEndPosition.X);
                OnRightWall |= Math.Round(MoverEndPosition.X) == Math.Round(ColliderPosition.X);
            }
        }
        private void ResolveStatic()
        {
            UpdateDifferenceWindow();
            if (Math.Abs(MoverColliderDifferenceWindow.X) < Math.Abs(MoverColliderDifferenceWindow.Y))
            {
                PositionOffset.X += MoverColliderDifferenceWindow.X;
                Velocity.X *= -Bounceback.X;
            } 
            else
            {
                PositionOffset.Y += MoverColliderDifferenceWindow.Y;
                Velocity.Y *= -Bounceback.Y;
            }
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
                ResolveStatic();
        }
        public override void Update(float delta)
        {
            base.Update(delta);
            OnFloor = OnCeiling = OnLeftWall = OnRightWall = false;
            ParentScale = Parent?.GlobalScale ?? new Vector2(1, 1);

            for (int i = 0; i < Iterations; i++)
            {
                if (Velocity == Vector2.Zero) break;
                PositionOffset += Velocity / Iterations / ParentScale;

                foreach (var body in Scene.PhysicsWorld)
                {
                    if (body.Parent == this) continue;

                    var bodyType = body.Parent?.GetType();
                    if (typeof(StaticHitbox).IsAssignableFrom(bodyType))
                    {
                        var collider = body.Parent as StaticHitbox;
                        foreach (var cBox in collider.Hitboxes)
                        {
                            foreach (var mBox in Hitboxes)
                            {
                                ResolveFor(mBox, cBox);
                                CheckCollisionBools();
                            }
                        }
                    }
                }
            }
        }
    }
}