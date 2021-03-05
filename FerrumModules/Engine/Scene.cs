using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace FerrumModules.Engine
{
    public class Scene : Entity
    {
        public List<Entity> DeletionQueue = new List<Entity>();
        public Camera Camera;
        public new FerrumEngine Engine;
        public Color BackgroundColor = Color.Gray;

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
