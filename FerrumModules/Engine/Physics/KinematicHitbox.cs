using System;
using Microsoft.Xna.Framework;
using Crossfrog.Ferrum.Engine.Modules;

namespace Crossfrog.Ferrum.Engine.Physics
{
    public class KinematicHitbox : StaticHitbox
    {
        public const int Iterations = 32;
        public const float MinDifferenceWindow = 0.5f;
        public const int VelocityRoundingPlaces = 4;
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
                if (Math.Abs(MoverColliderDifferenceWindow.X) > MinDifferenceWindow || Math.Abs(MoverColliderDifferenceWindow.Y) > MinDifferenceWindow)
                {
                    if (Math.Abs(MoverColliderDifferenceWindow.X) < Math.Abs(MoverColliderDifferenceWindow.Y))
                    {
                        ResolveX();
                        UpdateMoverPosition(mover);
                        if (MoverColliderIntersect())
                            ResolveY();
                    }
                    else
                    {
                        ResolveY();
                        UpdateMoverPosition(mover);
                        if (MoverColliderIntersect())
                            ResolveX();
                    }
                }
                else
                {
                    Velocity *= Bounceback;
                }    
            }

            UpdateMoverPosition(mover);
            ResolveStatic();
        }
        private void ResolveX()
        {
            Collision.Resolve1D(
                    MoverPosition.X,
                    MoverEndPosition.X,
                    ColliderPosition.X,
                    ColliderEndPosition.X,
                    ref Velocity.X,
                    ref PositionOffset.X,
                    ParentScale.X,
                    Bounceback.X);
        }
        private void ResolveY()
        {
            Collision.Resolve1D(
                    MoverPosition.Y,
                    MoverEndPosition.Y,
                    ColliderPosition.Y,
                    ColliderEndPosition.Y,
                    ref Velocity.Y,
                    ref PositionOffset.Y,
                    ParentScale.Y,
                    Bounceback.Y);
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
            if (MoverColliderIntersect())
            {
                UpdateDifferenceWindow();
                if (Math.Abs(MoverColliderDifferenceWindow.X) > Math.Abs(MoverColliderDifferenceWindow.Y))
                    PositionOffset.X += MoverColliderDifferenceWindow.X;
                else
                    PositionOffset.Y += MoverColliderDifferenceWindow.Y;
            }
        }
        public override void Update(float delta)
        {
            base.Update(delta);
            OnFloor = OnCeiling = OnLeftWall = OnRightWall = false;
            ParentScale = Parent?.GlobalScale ?? new Vector2(1, 1);

            Velocity = new Vector2(Misc.RoundedFloat(Velocity.X, VelocityRoundingPlaces), Misc.RoundedFloat(Velocity.Y, VelocityRoundingPlaces));

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