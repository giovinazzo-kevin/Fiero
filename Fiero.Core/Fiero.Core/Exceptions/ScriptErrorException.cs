namespace Fiero.Core.Exceptions
{
    public class ScriptErrorException : Exception
    {
        public ScriptErrorException(string value, string message)
            : base($"Script {value} threw error: {message}")
        {
        }
    }
}
