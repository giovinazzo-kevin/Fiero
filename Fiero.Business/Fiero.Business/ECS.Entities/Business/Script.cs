namespace Fiero.Business
{
    public class Script : Entity
    {
        [RequiredComponent]
        public ErgoScriptComponent ScriptProperties { get; private set; }
    }
}
