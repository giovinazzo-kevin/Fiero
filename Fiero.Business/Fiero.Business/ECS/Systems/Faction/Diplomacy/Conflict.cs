namespace Fiero.Business
{

    public readonly struct Conflict
    {
        public readonly ConflictName Name;
        public readonly ConflictMotiveName Motive;
        public readonly ConflictAgent A;
        public readonly ConflictAgent B;

        public Conflict(ConflictName name, ConflictMotiveName motive, ConflictAgent a, ConflictAgent b)
        {
            Name = name;
            Motive = motive;
            A = a;
            B = b;
        }
    }
}
