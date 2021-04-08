using System.Collections.Generic;
using Crossfrog.Ferrum.Engine.Modules;

namespace Crossfrog.Ferrum.Engine.Physics
{
    public class Sensor : PhysicsBody
    {
        public bool CollidesWith(PhysicsBody body)
        {
            foreach (var myShape in CollisionShapes)
                foreach (var bodyShape in body.CollisionShapes)
                    if (Collision.ConvexPolysCollide(myShape.GlobalVertices, bodyShape.GlobalVertices))
                        return true;
            return false;
        }
        public PhysicsBody[] CollidingBodies
        {
            get
            {
                var bodySet = new HashSet<PhysicsBody>();
                foreach (var shape in Scene.PhysicsWorld)
                {
                    if (shape.Parent != this && typeof(PhysicsBody).IsAssignableFrom(shape.Parent?.GetType()))
                    {
                        var body = shape.Parent as PhysicsBody;
                        if (CollidesWith(body))
                            bodySet.Add(body);
                    }
                }

                var bodyArray = new PhysicsBody[bodySet.Count];
                int i = 0;
                foreach (var body in bodySet)
                {
                    bodyArray[i] = body;
                    i++;
                }

                return bodyArray;
            }
        }
    }
}
