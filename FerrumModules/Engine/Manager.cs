using System;
using System.Collections.Generic;
using System.Text;

namespace FerrumModules.Engine
{
    public class Manager : ActiveObject
    {
        private Entity _entity;
        public Entity Entity
        {
            get => _entity;
            set
            {
                value.AssertManagerNameIsUnique(Name);
                AddObjectToList(value.Managers, this, "Manager added which already exists in the entity.");
                _entity?.RemoveManager(this);
                _entity = value;
            }
        }
        public Scene Scene => Entity.Scene;

        private string _name = "";
        public override string Name
        {
            get => _name;
            set
            {
                Entity.AssertManagerNameIsUnique(value);
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
