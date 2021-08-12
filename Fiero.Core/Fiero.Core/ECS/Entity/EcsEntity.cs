using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Fiero.Core
{
    [TransientDependency]
    public abstract class EcsEntity
    {
        internal Func<EcsEntity, int, bool> _refresh;
        internal Func<EcsEntity> _clone;
        internal Func<EcsEntity, Type, EcsEntity> _cast;

        public int Id { get; set; }

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

        public EcsEntity Clone() => _clone();

        public override int GetHashCode() => Id;
        public override bool Equals(object obj) => obj is EcsEntity other ? Id == other.Id : base.Equals(obj);
        public static bool operator ==(EcsEntity left, EcsEntity right)
        {
            return left?.Id == right?.Id;
        }
        public static bool operator !=(EcsEntity left, EcsEntity right)
        {
            return !(left == right);
        }
    }
}
