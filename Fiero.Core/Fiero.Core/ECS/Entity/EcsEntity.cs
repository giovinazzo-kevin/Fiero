using Ergo.Lang;

namespace Fiero.Core
{
    [TransientDependency]
    [Term(Marshalling = TermMarshalling.Named)]
    public abstract class EcsEntity
    {
        [NonTerm]
        internal Func<EcsEntity, int, bool, bool> _refresh;
        [NonTerm]
        internal Func<EcsEntity> _clone;
        [NonTerm]
        internal Func<EcsEntity, Type, EcsEntity> _cast;

        [Term]
        public int Id { get; set; }

        public bool TryRefresh(int newId, bool createRequiredComponents = false)
        {
            return _refresh(this, newId, createRequiredComponents);
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
