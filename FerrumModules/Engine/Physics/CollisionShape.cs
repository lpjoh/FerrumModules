using Microsoft.Xna.Framework;
namespace Crossfrog.Ferrum.Engine.Physics
{
    public abstract class CollisionShape : Entity
    {
        public override Entity Parent
        {
            set
            {
                Scene?.PhysicsWorld.Remove(this);
                base.Parent = value;
                Scene.PhysicsWorld.Add(this);
            }
        }
        public override void Exit()
        {
            base.Exit();
            Scene.PhysicsWorld.Remove(this);
        }
        public virtual Vector2[] GlobalVertices { get; }
        public virtual Rectangle BoundingBox { get; }
    }
}
