using Ergo.Shell;
using Ergo.Shell.Commands;
using LightInject;
using System.Text.RegularExpressions;
using LogLevel = Ergo.Shell.LogLevel;

namespace Fiero.Business;

[SingletonDependency]
public class SelectScript : ShellCommand
{
    private readonly IServiceFactory Services;

    public SelectScript(IServiceFactory services)
        : base(new[] { "select" }, "Loads the current knowledge base of a script to let you inspect it.", @"(?<script>.*)", true, 1000)
    {
        Services = services;
    }

    public override async IAsyncEnumerable<ShellScope> Callback(ErgoShell shell, ShellScope scope, Match match)
    {
        await Task.CompletedTask;
        var scripting = Services.GetInstance<GameSystems>().Scripting;
        var fuzz = match.Groups["script"].Value;
        var closest = scripting.Cache.Keys
            .Where(k => k.Contains(fuzz, StringComparison.OrdinalIgnoreCase))
            .OrderBy(k => k.Length);
        if (closest.FirstOrDefault() is not { } key)
        {
            shell.WriteLine($"Ambiguous match: {String.Join(",", closest)}", LogLevel.Err);
            yield return scope;
        }
        else
        {
            var val = scripting.Cache[key].ScriptProperties.KnowledgeBase;
            var scp = val.Scope;
            shell.WriteLine($"Selected script: {key}", LogLevel.Inf);
            yield return scope
                .WithKnowledgeBase(val)
                .WithInterpreterScope(scp);
        }
    }
}
