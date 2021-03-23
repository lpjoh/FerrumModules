using System;
using System.Collections.Generic;

using Crossfrog.Ferrum.Engine.Physics;

namespace Crossfrog.Ferrum.Engine
{
    public abstract class ActiveObject
    {
        protected string _name = "";
        public virtual string Name { get => _name; set => _name = value; }
        public bool Paused;

        private bool _initalized = false;
        public bool Initialized
        {
            get { return _initalized; }
            set { if (_initalized == false) _initalized = true; }
        }
        protected Entity _parent;
        public virtual Entity Parent { get => _parent; set => _parent = value; }

        public virtual void Init() { }
        public virtual void Update(float delta) { }
        public virtual void Exit() { }

        #region Active Object List Generics
        public ElementType GetFromObjectListByIndex<ElementType>(IList<ElementType> list, int index) where ElementType : ActiveObject
        {
            return list[index];
        }

        public ElementType GetFromObjectListByName<ElementType>(IList<ElementType> list, string elementName)
            where ElementType : ActiveObject
        {
            if (elementName == "") throw new Exception("You cannot fetch an object with no name from \"" + Name + "\".");

            foreach (var e in list)
                if (e.Name == elementName) return e;

#if DEBUG
            throw new Exception("Object \"" + elementName + "\" was requested from \"" + Name + "\", but did not exist.");
#endif
        }

#if DEBUG
        public void AssertNameIsUniqueInObjectList<ElementType>(IList<ElementType> list, string elementName) where ElementType : ActiveObject
        {
            foreach (var e in list)
            {
                if (!((e.Name == "") || (e.Name == null)) && (e.Name == elementName))
                    throw new Exception("An object named \"" + elementName + "\" already existed in \"" + Parent.Name + "\".");
            }
        }
#endif

        public bool ObjectListHas<ElementType>(IList<ElementType> list, ElementType element) where ElementType : ActiveObject
        {
            return list.Contains(element);
        }

        public bool ObjectListHas<ElementType>(IList<ElementType> list, string name) where ElementType : ActiveObject
        {
            foreach (var e in list) if (e.Name == name) return true;
            return false;
        }

        public NewObjectType AddObjectToList<ElementType, NewObjectType>(IList<ElementType> list, IList<ElementType> oldList, NewObjectType element)
            where ElementType : ActiveObject
            where NewObjectType : ElementType
        {
#if DEBUG
            if (ObjectListHas(list, element)) throw new Exception("Object \"" + element.Name + "\" added which already exists in \"" + Parent.Name + "\".");
            AssertNameIsUniqueInObjectList(list, element.Name);
#endif
            RemoveObjectFromList(oldList, element);
            if (typeof(CollisionShape).IsAssignableFrom(typeof(NewObjectType)))
            {
                var shape = element as CollisionShape;
                shape.Scene.PhysicsWorld.Remove(shape);
            }

            list.Add(element);

            if (!element.Initialized)
            {
                element.Init();
                element.Initialized = true;
            }

            return element;
        }

        public void RemoveObjectFromList<ElementType>(IList<ElementType> list, ElementType element)
            where ElementType : ActiveObject
        {
#if DEBUG
            if (list != null && !list.Remove(element))
                throw new Exception("Object \"" + element.Name + "\" does not exist in \"" + Name + "\" or was already removed.");
#else
            list?.Remove(element);
#endif
        }
        #endregion
    }
}
