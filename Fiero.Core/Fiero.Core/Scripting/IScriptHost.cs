namespace Fiero.Core
{
    public interface IScriptHost<TScripts>
        where TScripts : struct, Enum
    {
        bool TryLoad(TScripts fileName, out Script script);
    }
}
