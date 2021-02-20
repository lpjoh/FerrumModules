using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FerrumModules.Engine
{
    public abstract class Entity // TODO: Add sorting by layer on render layer index change
    {
        public virtual Vector2 PositionOffset { get; set; } = new Vector2(0.0f, 0.0f);
        public Vector2 GlobalPosition
        {
            get
            {
                if (Parent == null) return PositionOffset;
                return (Rotate(PositionOffset, Parent.GlobalPosition, GlobalAngle) + Parent.GlobalPosition) * GlobalScale;
            }
        }
        public virtual Vector2 ScaleOffset { get; set; } = new Vector2(1.0f, 1.0f);
        public Vector2 GlobalScale
        {
            get
            {
                if (Parent == null) return ScaleOffset;
                return Parent.GlobalScale * ScaleOffset;
            }
        }
        public virtual float AngleOffset { get; set; } = 0.0f;
        public float GlobalAngle
        {
            get
            {
                if (Parent == null) return AngleOffset;
                return Parent.GlobalAngle + AngleOffset;
            }
        }
        public Vector2 Rotate(Vector2 v, Vector2 center, float angle)
        {
            var ca = (float)Math.Cos(angle);
            var sa = (float)Math.Sin(angle);
            var difference = v - center;
            return new Vector2
                (
                    difference.X * ca - difference.Y * sa,
                    difference.Y * ca + difference.X * sa
                ) + center;
        }

        public List<Entity> Children { get; private set; } = new List<Entity>();
        public Entity Parent { get; private set; }
        public Entity RootEntity
        {
            get
            {
                if (Parent == null) return this;
                var rootEntity = Parent;
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

        public bool Centered = true;
        protected Vector2 RenderPosition { get; private set; }
        protected Vector2 RenderScale { get; private set; }
        protected float RenderAngle { get; private set; }

        private string _name = "";
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                Scene.RegisterName(this);
            }
        }

        public bool queueForDeletion = false;

        public int RenderLayer { get; private set; } = 0;

        public void SetRenderLayer<EnumType>(EnumType layerEnum)
        {
            if (!typeof(EnumType).IsEnum) { throw new ArgumentException(nameof(layerEnum)); }
            RenderLayer = (int)(object)layerEnum;
        }

        public virtual void Init() { }
        public virtual void Update(float delta)
        {
            foreach (var c in Children) c.Update(delta);
        }
        public virtual void Render(SpriteBatch spriteBatch, SpriteEffects spriteBatchEffects)
        {
            RenderScale = GlobalScale * Scene.Camera.GlobalScale;
            RenderPosition = (GlobalPosition - Scene.Camera.GlobalPosition) / GlobalScale * RenderScale;
            foreach (var c in Children) c.Render(spriteBatch, spriteBatchEffects);
        }
        public virtual void Exit()
        { 
            Scene.DeletionQueue.Add(this);
        }

        #region Child Management

        public bool HasChild<EntityType>(EntityType entity) where EntityType : Entity
        {
            return Children.Contains(entity);
        }

        public EntityType AddChild<EntityType>(EntityType entity) where EntityType : Entity
        {
            if (HasChild(entity)) throw new Exception("Child added which already exists in the parent.");

            entity.Parent?.RemoveChild(entity);
            Children.Add(entity);
            entity.Parent = this;

            entity.Init();
            return entity;
        }

        public void RemoveChild<EntityType>(EntityType entity) where EntityType : Entity
        {
            if (!Children.Remove(entity))
                throw new Exception("Entity \"" + entity.Name + "\" does not exist in the parent or was already removed.");
            if (Scene.HasName(entity.Name)) Scene.DeregisterName(entity.Name);
        }

        public List<EntityType> GetChildrenWithBase<EntityType>() where EntityType : Entity
        {
            var childrenWithBase = new List<EntityType>();
            foreach (var c in Children)
            {
                var type = c.GetType();
                if (type.IsSubclassOf(typeof(EntityType)) || type == typeof(EntityType)) childrenWithBase.Add((EntityType)c);
            }
            return childrenWithBase;
        }

        #endregion
    }
}
