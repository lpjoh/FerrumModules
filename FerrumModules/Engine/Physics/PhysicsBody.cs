using System.Collections.Generic;
using Crossfrog.Ferrum.Engine.Modules;

namespace Crossfrog.Ferrum.Engine.Physics
{
    public abstract class PhysicsBody : Entity
    {
        public virtual List<CollisionShape> CollisionShapes => Misc.OnlyWithBase<CollisionShape, Entity>(Children);
    }
}
