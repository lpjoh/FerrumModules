using System;
using System.Collections.Generic;
using System.Text;

namespace FerrumModules.Engine
{
    class FE_KinematicEntity : FE_PhysicsEntity
    {
        public FE_KinematicEntity(FE_TransformEntity puppetEntity) : base(puppetEntity) { }
        public override void Update(float delta)
        {
            PhysicsBody.SetAngularVelocity(0.0f);
            PhysicsBody.SetTransform(PhysicsBody.GetPosition(), 0.0f);
            base.Update(delta);
        }
    }
}
