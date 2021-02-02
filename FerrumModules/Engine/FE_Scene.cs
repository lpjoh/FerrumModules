using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;

using Box2DSharp.Dynamics;


namespace FerrumModules.Engine
{
    public class FE_Scene : FE_ActiveElement
    {
        public List<FE_Entity> EntityList { get; private set; } = new List<FE_Entity>();
        public Queue<FE_Entity> DeletionQueue = new Queue<FE_Entity>();

        private readonly Dictionary<string, FE_Entity> _entityNameDict = new Dictionary<string, FE_Entity>();
        public FE_Camera Camera = new FE_Camera();

        public World PhysicsWorld { get; private set; } = new World(new Vector2(0, 10));

        public FE_Scene()
        {
            Camera.Centered = false;
        }

        public override void Update(float delta)
        {
            PhysicsWorld.Step(1.0f / 60.0f, 6, 2);

            foreach (var e in DeletionQueue) Remove(e);
            DeletionQueue.Clear();

            foreach (var e in EntityList) e.Update(delta);
        }

        public override void Render(SpriteBatch spriteBatch, SpriteEffects spriteBatchEffects)
        {
            SortEntitiesByLayer();
            foreach (var e in EntityList)
            {
                e.Render(spriteBatch, spriteBatchEffects);
            }
        }

        private void SortEntitiesByLayer()
        {
            for (var i = 1; i < EntityList.Count; i++)
            {
                var entity = EntityList[i];
                var iterationComplete = false;
                for (var j = i - 1; j >= 0 && !iterationComplete;)
                {
                    if (entity.RenderLayer < EntityList[j].RenderLayer)
                    {
                        EntityList[j + 1] = EntityList[j];
                        j--;
                        EntityList[j + 1] = entity;
                    }
                    else iterationComplete = true;
                }
            }
        }

        #region Entity Management

        public EntityType Get<EntityType>(string entityName) where EntityType : FE_Entity
        {
            if (!Has(entityName)) throw new Exception("Entity \"" + entityName + "\" was requested, but did not exist.");
            return (EntityType)_entityNameDict[entityName];
        }

        public bool Has(string entityName)
        {
            return _entityNameDict.ContainsKey(entityName);
        }

        public bool Has<EntityType>(EntityType entity) where EntityType : FE_Entity
        {
            return EntityList.Contains(entity);
        }    

        public EntityType Add<EntityType>(EntityType entity) where EntityType : FE_Entity
        {
            if (Has(entity)) throw new Exception("Entity added which already exists in the scene.");
            EntityList.Add(entity);

            entity.Scene = this;
            entity.Init();
            return entity;
        }

        public void RegisterName(FE_Entity entity, string name)
        {
            if (!Has(entity)) throw new Exception("An entity outside of the current scene tried to register its name.");
            if (Has(name))
                throw new Exception
                    ("The entity name \"" + entity.Name + "\" already existed, and was overwritten.");

            if (!(entity.Name == "") && Has(entity.Name)) _entityNameDict.Remove(entity.Name);

            _entityNameDict[name] = entity;
        }

        public void Remove<EntityType>(EntityType entity) where EntityType : FE_Entity
        {
            if (!EntityList.Remove(entity))
                throw new Exception("Entity \"" + entity.Name + "\" does not exist in the scene or was already removed.");
            if (Has(entity.Name)) _entityNameDict.Remove(entity.Name);
        }

        public void Remove(string entityName)
        {
            if (!Has(entityName)) throw new Exception("Entity \"" + entityName + "\" was requested to be removed, but did not exist.");
            Get<FE_Entity>(entityName).Exit();
        }

        public List<EntityType> GetEntitiesWithBase<EntityType>() where EntityType : FE_Entity
        {
            var entitiesWithBase = new List<EntityType>();
            foreach (var e in EntityList)
            {
                if (e.GetType().IsSubclassOf(typeof(EntityType))) entitiesWithBase.Add((EntityType)e);
            }
            return entitiesWithBase;
        }

        #endregion
    }
}
