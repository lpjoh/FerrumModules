using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Crossfrog.Ferrum.Engine.Physics;

namespace Crossfrog.Ferrum.Engine
{
    public class Scene : Entity
    {
        public new FE_Engine Engine;

        public Color BackgroundColor = Color.Gray;
        public Camera Camera;

        public List<CollisionShape> PhysicsWorld = new List<CollisionShape>();

        public List<Entity> EntitiesToBeDeleted = new List<Entity>();
        public List<Component> ComponentsToBeDeleted = new List<Component>();

        public Scene()
        {
            Camera = AddChild(new Camera());
            Camera.Name = "Camera";
            Camera.Centered = false;
        }
        public override void Exit()
        {
#if DEBUG
            if (Scene == this) throw new Exception("Don't delete a scene while it's in use.");
#endif
        }
    }
}
