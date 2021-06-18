namespace Fiero.Core
{
    public interface IRandomNumber
    {
        int Range { get; }
        int Next();
    }
}
