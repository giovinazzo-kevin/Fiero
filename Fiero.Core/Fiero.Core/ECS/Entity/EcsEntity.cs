using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Fiero.Core
{
    [TransientDependency]
    public abstract class EcsEntity
    {
        internal Func<EcsEntity, int, bool> _refresh;
        internal Func<EcsEntity, Type, EcsEntity> _cast;

        public int Id { get; internal set; }

        public bool TryRefresh(int newId)
        {
            return _refresh(this, newId);
        }

        public bool TryCast<T>(out T newEntity)
            where T : EcsEntity
        {
            newEntity = (T)_cast(this, typeof(T));
            return newEntity != null;
        }
    }
}
