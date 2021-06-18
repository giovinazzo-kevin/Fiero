namespace Fiero.Business
{
    public readonly struct ConflictAgent
    {
        public readonly string Name;
        public readonly int[] EntityIds;
        public readonly bool IsCulpable;

        public ConflictAgent WithCulpability(bool culpable) => new(Name, culpable, EntityIds);

        public ConflictAgent(string name, bool culpable, params int[] entityIds)
        {
            Name = name;
            EntityIds = entityIds;
            IsCulpable = culpable;
        }

    }
}
