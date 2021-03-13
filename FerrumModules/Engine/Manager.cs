using System;
using System.Collections.Generic;
using System.Text;

namespace FerrumModules.Engine
{
    public class Manager : ActiveObject
    {
        public override Entity Parent
        {
            set
            {
                var oldSiblings = _parent?.Managers;
                _parent = value;
                AddObjectToList(value.Managers, oldSiblings, this);
            }
        }
        public Scene Scene => Parent.Scene;

        public override string Name
        {
            set
            {
                Parent.AssertManagerNameIsUnique(value);
                _name = value;
            }
        }

        public override void Exit()
        {
            base.Exit();
            Scene.ManagersToBeDeleted.Add(this);
        }
    }
}
