namespace Crossfrog.Ferrum.Engine
{
    public class Component : ActiveObject
    {
        public override Entity Parent
        {
            set
            {
                var oldSiblings = _parent?.Components;
                _parent = value;
                AddObjectToList(value.Components, oldSiblings, this);
            }
        }
        public Scene Scene => Parent.Scene;

        public override string Name
        {
            set
            {
#if DEBUG
                Parent?.AssertNameIsUniqueInObjectList(Parent.Components, value);
#endif
                _name = value;
            }
        }

        public override void Exit()
        {
            base.Exit();
            Scene.ComponentsToBeDeleted.Add(this);
        }
    }
}
