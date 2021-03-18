using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace FerrumModules.Engine
{
    public class Scene : Entity
    {
        public Camera Camera;
        public new FerrumEngine Engine;
        public Color BackgroundColor = Color.Gray;

        public List<Entity> EntitiesToBeDeleted = new List<Entity>();
        public List<Manager> ManagersToBeDeleted = new List<Manager>();

        public Scene()
        {
            Camera = AddChild(new Camera());
            Camera.Centered = false;
        }

        public override void Update(float delta)
        {
            base.Update(delta);

            foreach (var e in EntitiesToBeDeleted) e.Parent?.RemoveObjectFromList(e.Parent.Children, e);
            EntitiesToBeDeleted.Clear();
            foreach (var m in ManagersToBeDeleted) m.Parent?.RemoveObjectFromList(m.Parent.Managers, m);
            ManagersToBeDeleted.Clear();
        }

        public override void Exit() { if (Scene == this) throw new Exception("Don't delete a scene while it's in use."); }
    }
}
