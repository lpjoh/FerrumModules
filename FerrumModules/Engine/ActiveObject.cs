using System;
using System.Collections.Generic;

namespace FerrumModules.Engine
{
    public abstract class ActiveObject
    {
        public virtual string Name { get; set; }
        public bool Paused;

        private bool _initalized = false;
        public bool Initialized
        {
            get { return _initalized; }
            set { if (_initalized == false) _initalized = true; }
        }

        public virtual void Init() { }
        public virtual void Update(float delta) { }
        public virtual void Exit() { }

        #region Active Object List Generics
        protected ElementType GetFromObjectListByIndex<ElementType>(List<ElementType> list, int index) where ElementType : ActiveObject
        {
            return list[index];
        }

        protected ElementType GetFromObjectListByName<ElementType>(List<ElementType> list, string name, string noNameExceptionTest, string doesNotExistExceptionText)
            where ElementType : ActiveObject
        {
            if (name == "") throw new Exception(noNameExceptionTest);

            foreach (var e in list)
                if (e.Name == name) return e;

            throw new Exception(doesNotExistExceptionText);
        }

        protected void AssertNameIsUniqueInObjectList<ElementType>(List<ElementType> list, string name, string exceptionText) where ElementType : ActiveObject
        {
            foreach (var e in list)
            {
                if (e.Name != "" && e.Name == name) throw new Exception(exceptionText);
            }
        }

        protected bool ObjectListHas<ElementType>(List<ElementType> list, ElementType element) where ElementType : ActiveObject
        {
            return list.Contains(element);
        }

        protected bool ObjectListHas<ElementType>(List<ElementType> list, string name) where ElementType : ActiveObject
        {
            foreach (var e in list) if (e.Name == name) return true;
            return false;
        }

        protected NewObjectType AddObjectToList<ElementType, NewObjectType>(List<ElementType> list, NewObjectType element, string alreadyExistsExceptionText)
            where ElementType : ActiveObject
            where NewObjectType : ElementType
        {
            if (ObjectListHas(list, element)) throw new Exception(alreadyExistsExceptionText);
            list.Add(element);

            if (!element.Initialized)
            {
                element.Init();
                element.Initialized = true;
            }

            return element;
        }

        protected void RemoveObjectFromList<ElementType>(List<ElementType> list, ElementType element, string doesNotExistExceptionText)
            where ElementType : ActiveObject
        {
            if (!list.Remove(element))
                throw new Exception(doesNotExistExceptionText);
        }

        protected List<BaseType> GetObjectsFromListWithBase<ElementType, BaseType>(List<ElementType> list)
            where BaseType : ElementType
            where ElementType : ActiveObject
        {
            var elementsWithBase = new List<BaseType>();
            foreach (var e in list)
            {
                var type = e.GetType();
                if (type.IsSubclassOf(typeof(BaseType)) || type == typeof(BaseType)) elementsWithBase.Add((BaseType)e);
            }
            return elementsWithBase;
        }

        #endregion
    }
}
