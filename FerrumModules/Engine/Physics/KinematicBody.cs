using System;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using Crossfrog.Ferrum.Engine.Modules;

namespace Crossfrog.Ferrum.Engine.Physics
{
    public class KinematicBody : Collider
    {
        public const int Iterations = 8;
        public Vector2 Velocity;
        public bool OnFloor { get; private set; }
        public bool OnCeiling { get; private set; }
        public bool OnLeftWall { get; private set; }
        public bool OnRightWall { get; private set; }
        public bool OnWall => OnLeftWall || OnRightWall;
        public void ResolveFor(CollisionShape mover, CollisionShape collider)
        {
            var moverVertices = mover.GlobalVertices;
            var colliderVertices = collider.GlobalVertices;

            var resolution = Collision.ResolutionFor(moverVertices, colliderVertices);
            if (resolution == null)
                return;

            var offset = resolution.Value;

            OnFloor |= offset.Y < 0 && offset.Y <= offset.X;
            OnCeiling |= offset.Y > 0 && offset.Y >= offset.X;

            OnRightWall |= offset.X < 0 && offset.X <= offset.Y;
            OnLeftWall |= offset.X > 0 && offset.X >= offset.Y;

            GlobalPosition += offset;
            Velocity.X += offset.X / Iterations;
            Velocity.Y += offset.Y / Iterations;
        }
        public override void Update(float delta)
        {
            base.Update(delta);
            OnFloor = OnCeiling = OnLeftWall = OnRightWall = false;

            var oldVelocity = Vector2.Zero + Velocity;

            for (int i = 0; i < Iterations; i++)
            {
                GlobalPosition += oldVelocity / Iterations;

                foreach (var body in Scene.PhysicsWorld)
                {
                    if (body.Parent == this) continue;

                    var bodyType = body.Parent?.GetType();
                    if (typeof(Collider).IsAssignableFrom(bodyType))
                    {
                        var collider = body.Parent as Collider;
                        foreach (var mBox in CollisionShapes)
                        {
                            foreach (var cBox in collider.CollisionShapes)
                            {
                                ResolveFor(mBox, cBox);
                            }
                        }
                    }
                }
                if (Velocity == Vector2.Zero) break;
            }
        }
    }
}
