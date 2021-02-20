using System;
using System.Collections.Generic;
using System.Text;

namespace FerrumModules.Engine
{
    class KinematicBody : RigidBody
    {
        public KinematicBody() : base() { }
        public override void Update(float delta)
        {
            PhysicsBody.SetAngularVelocity(0.0f);
            PhysicsBody.SetTransform(PhysicsBody.GetPosition(), 0.0f);
            base.Update(delta);
        }
    }
}
