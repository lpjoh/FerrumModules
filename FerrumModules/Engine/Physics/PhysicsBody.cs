using System.Collections.Generic;

namespace Crossfrog.Ferrum.Engine.Physics
{
    public abstract class PhysicsBody : Entity
    {
        public virtual List<CollisionShape> CollisionShapes { get; }
    }
}
