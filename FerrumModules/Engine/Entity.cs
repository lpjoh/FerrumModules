using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Crossfrog.Ferrum.Engine.Modules;

namespace Crossfrog.Ferrum.Engine
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
                if (Parent == null)
                    return PositionOffsetRotated;

                else if (Parent == RootEntity)
                    return PositionOffset + Parent.PositionOffset;

                return PositionOffsetRotated + Parent.GlobalPosition;
            }
            set
            {
                if (Parent == null)
                    PositionOffset = Rotation.Rotate(value, -AngleOffset);
                else if (Parent == RootEntity)
                    PositionOffset = value - Parent.PositionOffset;
                else
                    PositionOffset = Rotation.Rotate(value / Parent.GlobalScale, -GlobalAngle) - Parent.GlobalPosition;
            }
        }
        public Vector2 ScaleOffset = new Vector2(1.0f, 1.0f);
        public virtual Vector2 GlobalScale
        {
            get
            {
                if (Parent == null)
                    return ScaleOffset;
                return ScaleOffset * Parent.GlobalScale;
            }
            set
            {
                if (Parent == null)
                    ScaleOffset = value;
                else
                    ScaleOffset = value / Parent.GlobalScale;
            }
        }
        public float AngleOffset = 0.0f;
        public virtual float GlobalAngle
        {
            get
            {
                if (Parent == null)
                    return AngleOffset;
                return AngleOffset + Parent.GlobalAngle;
            }
            set
            {
                if (Parent == null)
                    AngleOffset = value;
                else
                    AngleOffset = value - Parent.GlobalAngle;
            }
        }
        public virtual bool Centered { get; set; } = true;

        public Color ColorOffset = Color.White;
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
                    Misc.NormalizedByte(ColorOffset.A) * Misc.NormalizedByte(parentColor.A));
                return globalOpaque;
            }
        }
        public float OpacityOffset
        {
            get => Misc.NormalizedByte(ColorOffset.A);
            set => ColorOffset = new Color(ColorOffset, value);
        }
        public float GlobalOpacity
        {
            get => Misc.NormalizedByte(GlobalColor.A);
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

        public EntityType AddChild<EntityType>(EntityType entity) where EntityType : Entity
        {
            entity.Parent = this;
            return entity;
        }
        public EntityType[] AddChildren<EntityType>(params EntityType[] entityList) where EntityType : Entity
        {
            foreach (var e in entityList) AddChild(e);
            return entityList;
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
        public FE_Engine Engine => Scene.Engine;

        #endregion

        #region Components

        public List<Component> Components { get; private set; } = new List<Component>();

        public bool HasComponent(Component component)
        {
            return ObjectListHas(Components, component);
        }
        public bool HasComponent(string name)
        {
            return ObjectListHas(Components, name);
        }
        public ComponentType GetComponent<ComponentType>(int index) where ComponentType : Component
        {
            return (ComponentType)GetFromObjectListByIndex(Components, index);
        }
        public ComponentType GetComponent<ComponentType>(string componentName) where ComponentType : Component
        {
            return (ComponentType)GetFromObjectListByName(Components, componentName);
        }
        public ComponentType AddComponent<ComponentType>(ComponentType component) where ComponentType : Component
        {
            component.Parent = this;
            return component;
        }

        #endregion

        public override string Name
        {
            set
            {
#if DEBUG
                Parent?.AssertNameIsUniqueInObjectList(Parent.Children, value);
#endif
                _name = value;
            }
        }

        public Dictionary<string, string> SpawnProperties;

        private int _renderLayer;
        public int RenderLayer
        {
            get => _renderLayer;
            set
            {
                if (Parent != null)
                {
                    _renderLayer = value;
                    UpdateRenderOrder();
                }
            }
        }
        public void SetRenderLayer<EnumType>(EnumType layerEnum) where EnumType : Enum
        {
            RenderLayer = (int)(object)layerEnum;
        }

        private void UpdateRenderOrder()
        {
#if DEBUG
            if (Parent == null)
                throw new Exception("Entity \"" + Name + "\" needs a parent to sort siblings by render layer.");
#endif

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
            foreach (var c in Components)
                if (!c.Paused) c.Update(delta);
            foreach (var c in Children)
                if (!c.Paused) c.Update(delta);
        }
        public override void Exit()
        {
            base.Exit();

            foreach (var c in Children) c.Exit();
            foreach (var c in Components) c.Exit();
            Scene.EntitiesToBeDeleted.Add(this);
        }

        public bool Visible = true;
        public virtual void Render(SpriteBatch spriteBatch)
        {
            if (!Visible) return;
            foreach (var c in Children) c.Render(spriteBatch);
        }
        public virtual void PreCollision()
        {
            foreach (var c in Children) c.PreCollision();
        }
    }
}
