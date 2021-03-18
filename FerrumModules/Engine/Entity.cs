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
        public virtual bool Centered { get; set; } = true;

        public Color ColorOffset = Color.White;
        private float NormalizedByte(byte value) => ((float)value + 1) / 256;
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

        public float OpacityOffset
        {
            get => NormalizedByte(ColorOffset.A);
            set => ColorOffset = new Color(ColorOffset, value);
        }
        public float GlobalOpacity
        {
            get => NormalizedByte(GlobalColor.A);
        }

        #endregion

        #region Children

        public readonly List<Entity> Children = new List<Entity>();

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

        public EntityType GetChild<EntityType>(string entityName) where EntityType : Entity
        {
            return (EntityType)GetFromObjectListByName(Children, entityName);
        }

        public Entity this[string name] { get => GetChild<Entity>(name); }

        private void AssertChildNameIsUnique(string name)
        {
            AssertNameIsUniqueInObjectList(Children, name);
        }

        public EntityType AddChild<EntityType>(EntityType entity) where EntityType : Entity
        {
            entity.Parent = this;
            entity.UpdateRenderOrder();
            return entity;
        }

        public EntityType[] AddChildren<EntityType>(params EntityType[] entityList) where EntityType : Entity
        {
            foreach (var e in entityList) AddChild(e);
            return entityList;
        }

        public BaseType[] GetChildrenWithBase<BaseType>() where BaseType : Entity
        {
            return GetObjectsFromListWithBase<Entity, BaseType>(Children);
        }

        #endregion

        #region Parents
        public override Entity Parent
        {
            set
            {
                var oldSiblings = _parent?.Children;
                _parent = value;
                AddObjectToList(value.Children, oldSiblings, this);
            }
        }
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
        public Scene Scene => RootEntity as Scene;
        public FerrumEngine Engine => Scene.Engine;

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

        public ManagerType GetManager<ManagerType>(string managerName) where ManagerType : Manager
        {
            return (ManagerType)GetFromObjectListByName(Managers, managerName);
        }

        public void AssertManagerNameIsUnique(string name)
        {
            AssertNameIsUniqueInObjectList(Managers, name);
        }

        public ManagerType AddManager<ManagerType>(ManagerType manager) where ManagerType : Manager
        {
            manager.Parent = this;
            return manager;
        }

        #endregion

        public override string Name
        {
            set
            {
                Parent?.AssertChildNameIsUnique(value);
                _name = value;
            }
        }

        public Dictionary<string, string> SpawnProperties;

        private int RenderLayer = 0;
        public void SetRenderLayer<EnumType>(EnumType layerEnum) where EnumType : Enum
        {
            if (Parent != null)
            {
                RenderLayer = (int)(object)layerEnum;
                UpdateRenderOrder();
            }
        }

        private void UpdateRenderOrder()
        {
            Parent.Children.Remove(this);
            var siblings = Parent.Children;

            for (int i = 0; i < siblings.Count; i++)
            {
                var sibling = Parent.Children[i];
                if (sibling.RenderLayer > RenderLayer)
                {
                    siblings.Insert(i, this);
                    return;
                }
            }
            siblings.Add(this);
            return;
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            foreach (var m in Managers)
                if (!m.Paused) m.Update(delta);
            foreach (var c in Children)
                if (!c.Paused) c.Update(delta);
        }
        public override void Exit()
        {
            base.Exit();

            foreach (var c in Children) c.Exit();
            foreach (var m in Managers) m.Exit();
            Scene.EntitiesToBeDeleted.Add(this);
        }

        public bool Visible = true;
        public virtual void Render(SpriteBatch spriteBatch)
        {
            if (!Visible) return;
            foreach (var c in Children) c.Render(spriteBatch);
        }
    }
}
