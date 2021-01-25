using Microsoft.Xna.Framework;

namespace FerrumModules.Engine
{
    public class FE_Camera : FE_TransformEntity
    {
        public FE_TransformEntity PivotEntity;
        public Rectangle BoundingBox;

        public override void Init()
        {
            if (Scene.Camera == null) Scene.Camera = this;
        }

        public override void Update(float delta)
        {
            Position = PivotEntity.Position == null ? Position : PivotEntity.Position;
            base.Update(delta);
        }
    }
}
