namespace Fiero.Core.Exceptions
{
    public class ScriptNotFoundException : Exception
    {
        public ScriptNotFoundException(string value)
            : base($"Script {value} was not found")
        {
        }
    }
}
