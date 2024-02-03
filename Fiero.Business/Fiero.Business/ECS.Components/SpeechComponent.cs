namespace Fiero.Business
{
    public class SpeechComponent : EcsComponent
    {
        public readonly record struct SpeechDef(string Option, int Weight);

        public readonly Dictionary<string, List<SpeechDef>> Phrases = new();
    }
}
