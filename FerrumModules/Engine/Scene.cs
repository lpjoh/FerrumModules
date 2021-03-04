using System;
using System.Collections.Generic;

namespace FerrumModules.Engine
{
    public class Scene : Entity
    {
        public List<Entity> DeletionQueue = new List<Entity>();
        public Camera Camera;
        public new Engine Engine;

        public Scene()
        {
            Camera = AddChild(new Camera());
            Camera.Centered = false;
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            foreach (var e in DeletionQueue)
            {
                e.Parent?.RemoveChild(e);
            }
            
            DeletionQueue.Clear();
        }
    }
}
