using System.Text.Json;

namespace Fiero.Business
{
    [SingletonDependency]
    public class GameDialogues
    {
        protected GameLocalizations<LocaleName> Localizations;
        protected readonly Dictionary<string, DialogueNode> DialogueNodes = new();

        public DialogueNode GetDialogue(string id) => DialogueNodes.TryGetValue(id, out var node) ? node : null;

        public GameDialogues(GameLocalizations<LocaleName> localizations)
        {
            Localizations = localizations;
        }

        public void LoadDialogues()
        {
            Localizations.CurrentCulture = LocaleName._Dialogue;
            if (!Localizations.TryGet<JsonElement>(string.Empty, out var elem))
            {
                throw new ArgumentException();
            }
            Localizations.CurrentCulture = Localizations.DefaultCulture;
            var definitions = elem.EnumerateObject()
                .Select(prop =>
                {
                    if (!(prop.Value.TryGetProperty("Face", out var faceProp)
                        && faceProp.GetString() is { } face))
                    {
                        face = "GKR_Calm";
                    }
                    if (!(prop.Value.TryGetProperty("Lines", out var linesProp)
                        && linesProp.EnumerateArray().Select(x => x.GetString()) is { } lines))
                    {
                        lines = Enumerable.Empty<string>();
                    }
                    if (!(prop.Value.TryGetProperty("Cancellable", out var cancellableProp)
                        && cancellableProp.GetBoolean() is { } cancellable))
                    {
                        cancellable = false;
                    }
                    if (!(prop.Value.TryGetProperty("Choices", out var choicesProp)
                        && choicesProp.EnumerateArray().Select(x =>
                        {
                            if (!(x.TryGetProperty("Line", out var lineProp)
                                && lineProp.GetString() is { } line))
                            {
                                line = String.Empty;
                            }
                            if (!(x.TryGetProperty("Next", out var nextProp)
                                && nextProp.GetString() is { } next))
                            {
                                next = String.Empty;
                            }
                            return (Line: line, Next: next);
                        }) is { } choices))
                    {
                        choices = Enumerable.Empty<(string, string)>();
                    }
                    if (!(prop.Value.TryGetProperty("Next", out var nextProp)
                        && nextProp.GetString() is { } next))
                    {
                        next = String.Empty;
                    }
                    if (!(prop.Value.TryGetProperty("Title", out var titleProp)
                        && titleProp.GetString() is { } title))
                    {
                        title = String.Empty;
                    }
                    var lTitle = Localizations.Translate(title);
                    var lLines = lines.Select(Localizations.Translate).ToArray();
                    var lChoices = choices.Select(x => (Localizations.Translate(x.Line), x.Next)).ToArray();
                    return new DialogueNodeDefinition(prop.Name, face, lTitle, lLines, cancellable, lChoices, next);
                })
                .ToDictionary(x => x.Id);
            var nodes = definitions.Values.Select(d => new DialogueNode(d.Id, d.Face, d.Title, d.Lines, d.Cancellable))
                .ToDictionary(x => x.Id);
            foreach (var node in nodes.Values)
            {
                if (!String.IsNullOrWhiteSpace(definitions[node.Id].Next))
                {
                    node.Next = nodes[definitions[node.Id].Next];
                }
                foreach (var choice in definitions[node.Id].Choices)
                {
                    if (!String.IsNullOrWhiteSpace(choice.Next) && nodes.TryGetValue(choice.Next, out var nextNode))
                    {
                        node.Choices[choice.Line] = nextNode;
                    }
                    else
                    {
                        node.Choices[choice.Line] = null;
                    }
                }
            }
            foreach (var (key, value) in nodes)
            {
                DialogueNodes.Add(key, value);
            }
        }
    }
}
