using System;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Dynamics;

namespace FerrumModules.Engine
{
    class RigidBody : Entity
    {
        protected Body PhysicsBody;

        public Microsoft.Xna.Framework.Vector2 Velocity
        {
            get
            {
                var linearVelocity = PhysicsBody.LinearVelocity;
                return new Microsoft.Xna.Framework.Vector2(linearVelocity.X, linearVelocity.Y);
            }
            set { PhysicsBody.SetLinearVelocity(new System.Numerics.Vector2(value.X, value.Y)); }
        }

        public override void Init()
        {
            base.Init();

            var bodyDef = new BodyDef { BodyType = BodyType.DynamicBody };

            PhysicsBody = Scene.PhysicsWorld.CreateBody(bodyDef);

            PolygonShape dynamicBox = new PolygonShape();
            dynamicBox.SetAsBox(8.0f, 8.0f);

            FixtureDef fixtureDef = new FixtureDef
            {
                Shape = dynamicBox,
                Density = 0.5f,
                Friction = 0.3f
            };

            PhysicsBody.CreateFixture(fixtureDef);
        }
        public override void Exit()
        {
            Scene.PhysicsWorld.BodyList.Remove(PhysicsBody);
        }
    }
}
