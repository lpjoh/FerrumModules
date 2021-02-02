using System;

namespace FerrumModules.Engine
{
    public abstract class FE_Entity : FE_ActiveElement
    {
        private FE_Scene _scene;
        public FE_Scene Scene
        {
            get { return _scene; }
            set
            {
                if (_scene == null) _scene = value;
            }
        }

        private string _name = "";
        public string Name
        {
            get { return _name; }
            set
            {
                Scene.RegisterName(this, value);
                _name = value;
            }
        }

        public bool queueForDeletion = false;

        public int RenderLayer { get; private set; } = 0;

        public void SetRenderLayer<EnumType>(EnumType layerEnum)
        {
            if (!typeof(EnumType).IsEnum) { throw new ArgumentException(nameof(layerEnum)); }
            RenderLayer = (int)(object)layerEnum;
        }

        public virtual void Exit() { Scene.DeletionQueue.Enqueue(this); }
    }
}
