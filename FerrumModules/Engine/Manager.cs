namespace Crossfrog.FerrumEngine
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
#if DEBUG
                Parent?.AssertNameIsUniqueInObjectList(Parent.Managers, value);
#endif
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
