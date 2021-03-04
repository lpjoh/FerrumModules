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
            get { return _entity; }
            set
            {
                _entity.RemoveManager(this);
                _entity = value;
                if (!Initialized)
                {
                    Init();
                    Initialized = true;
                }
                _entity.AddManager(this);
            }
        }
        private string _name = "";
        public override string Name
        {
            get { return _name; }
            set
            {
                Entity.AssertManagerNameIsUnique(value);
                _name = value;
            }
        }
    }
}
