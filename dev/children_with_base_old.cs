var childrenWithBase = new List<EntityType>();
            foreach (var c in Children)
            {
                var type = c.GetType();
                if (type.IsSubclassOf(typeof(EntityType)) || type == typeof(EntityType)) childrenWithBase.Add((EntityType)c);
            }
            return childrenWithBase;