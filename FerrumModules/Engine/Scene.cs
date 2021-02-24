using System;
using System.Collections.Generic;
using System.Numerics;

using Box2DSharp.Dynamics;
using Box2DSharp.Collision.Shapes;


namespace FerrumModules.Engine
{
    public class Scene : Entity
    {
        private readonly Dictionary<string, Entity> _childNameDict = new Dictionary<string, Entity>();
        public List<Entity> DeletionQueue = new List<Entity>();
        public Camera Camera;
        public World PhysicsWorld { get; private set; } = new World(new Vector2(0, 100));

        public Scene()
        {
            Camera = Scene.AddChild(new Camera());
            Camera.Centered = false;
        }

        #region Entity Name Management

        public EntityType GetByName<EntityType>(string entityName) where EntityType : Entity
        {
            if (!HasName(entityName)) throw new Exception("Entity \"" + entityName + "\" was requested, but did not exist.");
            return (EntityType)_childNameDict[entityName];
        }

        public bool HasName(string entityName)
        {
            return _childNameDict.ContainsKey(entityName);
        }
        public void RegisterName(Entity entity)
        {
            if (HasName(entity.Name))
                throw new Exception ("The entity name \"" + entity.Name + "\" already existed, and was overwritten.");

            _childNameDict[entity.Name] = entity;
        }
        public void DeregisterName(string entityName)
        {
            if (!HasName(entityName))
                throw new Exception("The entity name \"" + entityName + "\" was requested to be removed, but did not exist.");

            _childNameDict.Remove(entityName);
        }
        #endregion

        public override void Update(float delta)
        {
            base.Update(delta);
            foreach (var e in DeletionQueue)
            {
                if (HasName(e.Name)) DeregisterName(e.Name);
                e.Parent?.RemoveChild(e);
            }
            
            DeletionQueue.Clear();
            PhysicsWorld.Step(delta, 6, 2);
        }
    }
}
