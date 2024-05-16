using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Fiero.Core.Ergo;
using Fiero.Core.Exceptions;

namespace Fiero.Core.Extensions
{
    public static class EntityBuilderExtensions
    {
        public static IEntityBuilder<T> LoadState<T>(this IEntityBuilder<T> builder, string resourceName)
            where T : EcsEntity
        {
            var scripts = builder.Entities.ServiceFactory.GetInstance<GameScripts>();
            if (!scripts.TryGet<ErgoScript>(CoreScriptName.Entity, out var entities))
                throw new ScriptNotFoundException(CoreScriptName.Entity);
            entities.VM.Query = entities.VM.ParseAndCompileQuery($"dict({resourceName.ToErgoCase()}, X)");
            // X == {prop: prop_component{}, other_prop: other_prop_component{}, ...}
            var var_x = new Variable("X");
            foreach (var solution in entities.VM.RunInteractive())
            {
                var X = solution.Substitutions[var_x].Substitute(solution.Substitutions);
                if (X is not Set set)
                    throw new ScriptErrorException(CoreScriptName.Entity, $"Expected term of type {nameof(Set)}, found: {X.Explain()}");
                var kvps = set.Contents
                    .ToDictionary(a => (Atom)((Complex)a).Arguments[0], a => ((Complex)a).Arguments[1]);
                foreach (var prop in builder.Entities.GetProxyableProperties<T>())
                {
                    var name = new Atom(prop.Name.ToErgoCase());
                    if (kvps.TryGetValue(name, out var dict))
                    {
                        builder = builder.Load(prop.PropertyType, (Dict)dict);
                    }
                }
            }
            return builder;
        }
    }
}
