using System.Text.Json;

namespace Fiero.Business
{
    [SingletonDependency]
    public class GameDialogues
    {
        protected GameLocalizations<LocaleName> Localizations;
        protected readonly Dictionary<string, Dictionary<string, DialogueNode>> DialogueNodes;

        public DialogueNode GetDialogue(string owner, string id) => DialogueNodes.TryGetValue(owner, out var dict)
            && dict.TryGetValue(id, out var node) ? node : null;

        public DialogueNode GetDialogue<T>(NpcName owner, T id) where T : struct, Enum => GetDialogue(owner.ToString(), id.ToString());
        public DialogueNode GetDialogue<T>(FeatureName owner, T id) where T : struct, Enum => GetDialogue(owner.ToString(), id.ToString());

        public GameDialogues(GameLocalizations<LocaleName> localizations)
        {
            DialogueNodes = new Dictionary<string, Dictionary<string, DialogueNode>>();
            Localizations = localizations;
        }

        protected void LoadDialogues(string actor)
        {
            if (!Localizations.TryGet<JsonElement>($"Dialogue.{actor}", out var elem))
            {
                throw new ArgumentException(nameof(actor));
            }
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
                    return new DialogueNodeDefinition(prop.Name, face, title, lines.ToArray(), cancellable, choices.ToArray(), next);
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
                    if (!String.IsNullOrWhiteSpace(choice.Next))
                    {
                        node.Choices[choice.Line] = nodes[choice.Next];
                    }
                    else
                    {
                        node.Choices[choice.Line] = null;
                    }
                }
            }
            DialogueNodes.Add(actor, nodes);
        }

        public void LoadActorDialogues(NpcName actor)
        {
            LoadDialogues(actor.ToString());
        }

        public void LoadFeatureDialogues(FeatureName feature)
        {
            LoadDialogues(feature.ToString());
        }
    }
}
