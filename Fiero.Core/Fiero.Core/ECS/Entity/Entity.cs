using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Fiero.Core
{
    public abstract class Entity
    {
        internal Func<Entity, int, bool> _refresh;
        internal Func<Entity, Type, Entity> _cast;

        public int Id { get; internal set; }

        public bool TryRefresh(int newId)
        {
            return _refresh(this, newId);
        }

        public bool TryCast<T>(out T newEntity)
            where T : Entity
        {
            newEntity = (T)_cast(this, typeof(T));
            return newEntity != null;
        }
    }
}
