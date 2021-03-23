﻿using System.Linq;
using System.Collections.Generic;

using Crossfrog.Ferrum.Engine.Modules;

namespace Crossfrog.Ferrum.Engine.Physics
{
    public abstract class HitboxCollider : PhysicsBody
    {
        public List<HitboxShape> Hitboxes => Misc.OnlyWithBase<HitboxShape, Entity>(Children);
        public override List<CollisionShape> CollisionShapes => Hitboxes.Cast<CollisionShape>().ToList();
    }
}