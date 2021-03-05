using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FerrumModules.Engine
{
    public class Entity : ActiveObject
    {
        #region Transformations
        public Vector2 PositionOffset = Vector2.Zero;
        public Vector2 PositionOffsetRotated
        {
            get
            {
                if (Parent == null) return Rotation.Rotate(PositionOffset * ScaleOffset, AngleOffset);
                return Rotation.Rotate(PositionOffset * Parent.GlobalScale, GlobalAngle);
            }
        }
        public Vector2 GlobalPositionNoOffset
        {
            get
            {
                if (Parent == null) return Vector2.Zero;
                return Parent.GlobalPosition;
            }
        }
        public virtual Vector2 GlobalPosition
        {
            get
            {
                if (Parent == null) return PositionOffsetRotated;
                var positionOffset = Parent == RootEntity ? PositionOffset : PositionOffsetRotated;
                return Parent.GlobalPosition + positionOffset;
            }
        }
        public Vector2 ScaleOffset = new Vector2(1.0f, 1.0f);
        public virtual Vector2 GlobalScale
        {
            get
            {
                if (Parent == null) return ScaleOffset;
                return ScaleOffset * Parent.GlobalScale;
            }
        }
        public float AngleOffset = 0.0f;
        public virtual float GlobalAngle
        {
            get
            {
                if (Parent == null) return AngleOffset;
                return Parent.GlobalAngle + AngleOffset;
            }
        }
        protected Vector2 RenderPosition { get; private set; }
        protected Vector2 RenderScale { get; private set; }
        protected float RenderAngle { get; private set; }
        public virtual bool Centered { get; set; } = true;

        public Color ColorOffset = Color.White;
        private float NormalizedByte(byte value) { return ((float)value + 1) / 256; }
        public virtual Color GlobalColor
        {
            get
            {
                if (Parent == null) return ColorOffset;

                var parentColor = Parent.GlobalColor;

                var globalOpaque = new Color(
                    ColorOffset.R * parentColor.R,
                    ColorOffset.G * parentColor.G,
                    ColorOffset.B * parentColor.B,
                    NormalizedByte(ColorOffset.A) * NormalizedByte(parentColor.A));
                return globalOpaque;
            }
        }

        #endregion

        #region Children

        public List<Entity> Children { get; private set; } = new List<Entity>();

        public bool HasChild(Entity entity)
        {
            return ObjectListHas(Children, entity);
        }

        public bool HasChild(string name)
        {
            return ObjectListHas(Children, name);
        }

        public EntityType GetChild<EntityType>(int index) where EntityType : Entity
        {
            return (EntityType)GetFromObjectListByIndex(Children, index);
        }

        public Entity this[int i] { get => GetChild<Entity>(i); }

        public EntityType GetChild<EntityType>(string name) where EntityType : Entity
        {
            return (EntityType)GetFromObjectListByName(
                Children, name,
                "You cannot fetch an entity with no name.",
                "Entity \"" + name + "\" was requested, but did not exist.");
        }

        public Entity this[string name] { get => GetChild<Entity>(name); }

        private void AssertChildNameIsUnique(string name)
        {
            AssertNameIsUniqueInObjectList(Children, name, "An entity named \"" + name + "\" already existed in the parent.");
        }

        public EntityType AddChild<EntityType>(EntityType entity) where EntityType : Entity
        {
            AddObjectToList(Children, entity, "Child added which already exists in the parent.");
            entity.Parent?.RemoveChild(entity);
            entity.Parent = this;
            return entity;
        }

        public void RemoveChild<EntityType>(EntityType entity) where EntityType : Entity
        {
            RemoveObjectFromList(Children, entity, "Entity \"" + entity.Name + "\" does not exist in the parent or was already removed.");
        }

        public List<EntityType> GetChildrenWithBase<EntityType>() where EntityType : Entity
        {
            return GetObjectsFromListWithBase<Entity, EntityType>(Children);
        }

        #endregion

        #region Parents

        public Entity Parent { get; private set; }
        public Entity RootEntity
        {
            get
            {
                if (Parent == null) return this;
                var rootEntity = this;
                while (rootEntity.Parent != null)
                {
                    rootEntity = rootEntity.Parent;
                }
                return rootEntity;
            }
        }
        public Scene Scene
        {
            get { return RootEntity as Scene; }
        }
        public FerrumEngine Engine
        {
            get { return Scene.Engine; }
        }

        #endregion

        #region Managers

        public List<Manager> Managers { get; private set; } = new List<Manager>();

        public bool HasManager(Manager manager)
        {
            return ObjectListHas(Managers, manager);
        }
        public bool HasManager(string name)
        {
            return ObjectListHas(Managers, name);
        }

        public ManagerType GetManager<ManagerType>(int index) where ManagerType : Manager
        {
            return (ManagerType)GetFromObjectListByIndex(Managers, index);
        }

        public ManagerType GetManager<ManagerType>(string name) where ManagerType : Manager
        {
            return (ManagerType)GetFromObjectListByName(
                Managers, name,
                "You cannot fetch a manager with no name.",
                "Manager \"" + name + "\" was requested, but did not exist.");
        }

        public void AssertManagerNameIsUnique(string name)
        {
            AssertNameIsUniqueInObjectList(Managers, name, "A manager named \"" + name + "\" already existed in the entity.");
        }

        public ManagerType AddManager<ManagerType>(ManagerType manager) where ManagerType : Manager
        {
            manager.Entity = this;
            return manager;
        }

        public void RemoveManager<ManagerType>(ManagerType manager) where ManagerType : Manager
        {
            RemoveObjectFromList(Managers, manager, "Manager \"" + manager.Name + "\" does not exist in the entity or was already removed.");
        }

        #endregion

        private string _name = "";
        public override string Name
        {
            get { return _name; }
            set
            {
                Parent?.AssertChildNameIsUnique(value);
                _name = value;
            }
        }

        public float RenderLayerOffset { get; private set; } = 1.0f;
        public float GlobalRenderLayer
        {
            get
            {
                if (Parent == null) return RenderLayerOffset;

                var globalRenderLayer = 1.0f;
                var workingParent = this;
                while (workingParent.Parent != null)
                {
                    if (workingParent.RenderLayerOffset < globalRenderLayer) globalRenderLayer = workingParent.RenderLayerOffset;
                    workingParent = workingParent.Parent;
                }
                return globalRenderLayer;
            }
        }

        public void SetRenderLayer<EnumType>(EnumType layerEnum) where EnumType : Enum
        {
            RenderLayerOffset = (float)((int)(object)layerEnum + 1) / Enum.GetNames(typeof(EnumType)).Length;
        }

        public bool Visible = true;

        public override void Init() { }
        public override void Update(float delta)
        {
            foreach (var m in Managers) m.Update(delta);
            foreach (var c in Children) c.Update(delta);
        }
        public override void Exit()
        {
            Scene.DeletionQueue.Add(this);
            foreach (var c in Children) c.Exit();
        }
        public virtual void Render(SpriteBatch spriteBatch, SpriteEffects spriteBatchEffects)
        {
            if (!Visible) return;
            foreach (var c in Children)
            {
                c.Render(spriteBatch, spriteBatchEffects);
            }

            if (GetType().GetMethod("Render").DeclaringType == typeof(Entity)) return;

            var camera = Scene.Camera;
            RenderScale = GlobalScale * camera.Zoom;
            RenderAngle = GlobalAngle + camera.AngleOffset;
            RenderPosition = Rotation.Rotate((GlobalPosition - camera.GlobalPosition) / GlobalScale * RenderScale, camera.AngleOffset);
        }
    }
}
