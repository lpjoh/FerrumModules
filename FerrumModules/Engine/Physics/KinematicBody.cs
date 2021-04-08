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
        private Vector2 ParentScale;
        public Vector2? ResolveFor(CollisionShape mover, CollisionShape collider)
        {
            var moverVertices = mover.GlobalVertices;
            var colliderVertices = collider.GlobalVertices;

            var resolution = Collision.ResolutionFor(moverVertices, colliderVertices);
            if (resolution == null)
                return null;

            var offset = resolution.Value / ParentScale;
            PositionOffset += offset;
            return offset;
        }
        public override void Update(float delta)
        {
            base.Update(delta);
            OnFloor = OnCeiling = OnLeftWall = OnRightWall = false;
            ParentScale = Parent?.GlobalScale ?? new Vector2(1, 1);

            for (int i = 0; i < Iterations; i++)
            {
                PositionOffset += Velocity / Iterations / ParentScale;

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
                                var resolution = ResolveFor(mBox, cBox);
                                if (resolution == null)
                                    continue;

                                if (typeof(KinematicBody).IsAssignableFrom(bodyType))
                                {
                                    var kinematic = collider as KinematicBody;
                                    if (Math.Abs(resolution.Value.X) > Math.Abs(resolution.Value.Y))
                                        Velocity.X = kinematic.Velocity.X;
                                    else
                                        Velocity.Y = kinematic.Velocity.Y;
                                }
                                else
                                {
                                    if (Math.Abs(resolution.Value.X) > Math.Abs(resolution.Value.Y))
                                        Velocity.X = 0;
                                    else
                                        Velocity.Y = 0;
                                }
                            }
                        }
                    }
                }
                if (Velocity == Vector2.Zero) break;
            }
        }
    }
}
