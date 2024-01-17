namespace Fiero.Business
{
    public class InfoComponent : EcsComponent
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class SpeechComponent : EcsComponent
    {
        public readonly record struct SpeechDef(string Option, int Weight);

        public readonly Dictionary<string, List<SpeechDef>> Phrases = new();
    }
}
