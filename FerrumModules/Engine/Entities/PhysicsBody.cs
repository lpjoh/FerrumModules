using System.Collections.Generic;

namespace Crossfrog.Ferrum.Engine.Entities
{
    public abstract class PhysicsBody : Entity
    {
        public List<CollisionShape> CollisionShapes
        {
            get
            {
                var shapeList = new List<CollisionShape>();
                
                foreach (var c in Children)
                    if (typeof(CollisionShape).IsAssignableFrom(c.GetType()))
                        shapeList.Add((CollisionShape)c);

                return shapeList;
            }
        }
    }
}
