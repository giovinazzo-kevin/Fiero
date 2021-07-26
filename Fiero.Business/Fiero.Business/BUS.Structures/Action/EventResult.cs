namespace Fiero.Business
{
    public class EventResult
    {
        public readonly bool Result;

        public EventResult(bool result)
        {
            Result = result;
        }

        public static implicit operator bool(EventResult r) => r.Result;
        public static implicit operator EventResult(bool b) => new(b);

    }
}
