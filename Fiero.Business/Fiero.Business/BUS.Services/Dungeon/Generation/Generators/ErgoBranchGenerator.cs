using Ergo.Interpreter.Libraries;
using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;

namespace Fiero.Business
{
    [SingletonDependency]
    public class ErgoBranchGenerator(GameScripts<ScriptName> scripts) : IBranchGenerator
    {
        [Term(Marshalling = TermMarshalling.Named)]
        public readonly record struct Step(FloorId FloorId, Coord Position, Coord Size);
        [Term(Marshalling = TermMarshalling.Named)]
        public readonly record struct PlacePrefabArgs(
            int Id, bool MirrorY, bool MirrorX, int Rotate
        );
        [Term(Marshalling = TermMarshalling.Named)]
        public readonly record struct Prefab(
            string Name, Coord Size, ITerm[][] Canvas
        );

        public readonly GameScripts<ScriptName> Scripts = scripts;
        public readonly DungeonTheme Theme = DungeonTheme.Default;

        public readonly Hook GenerateHook = new(new(new("generate"), 2, FieroLib.Modules.Map, default));
        public readonly Hook GetPrefabsHook = new(new(new("get_prefabs"), 1, FieroLib.Modules.Map, default));

        public Floor GenerateFloor(FloorId id, FloorBuilder builder)
        {
            var script = (ErgoScript)Scripts.Get(ScriptName.Map_Test);
            var fieroLib = script.VM.KB.Scope.GetLibrary<FieroLib>(FieroLib.Modules.Fiero);
            var map = fieroLib.Maps[script.VM.KB.Scope.Entry];
            var step = new Step(id, Coord.Zero, map.Size - Coord.PositiveOne);
            var arg = TermMarshall.ToTerm(step);
            return builder
                .WithStep(ctx =>
                {
                    var var_prefabs = new Variable("Prefabs");
                    GetPrefabsHook.SetArg(0, var_prefabs);
                    script.VM.Query = GetPrefabsHook.Compile();
                    script.VM.Run();
                    var prefabs = new Dictionary<string, Prefab>();
                    if (script.VM.TryPopSolution(out var sol)
                        && sol.Substitutions[var_prefabs] is List lst)
                    {
                        foreach (var item in lst.Contents)
                        {
                            if (!item.Matches<Prefab>(out var prefab)
                            || prefab.Size.X * prefab.Size.Y != prefab.Canvas.Length)
                            {
                                Throw("Invalid prefab.");
                                // TODO: Log
                                return;
                            }
                            if (prefabs.ContainsKey(prefab.Name))
                            {
                                Throw("Prefab with the same name already exists.");
                                // TODO: Log
                                return;
                            }
                            prefabs.Add(prefab.Name, prefab);
                        }
                    }
                    var var_geometry = new Variable("Geometry");
                    GenerateHook.SetArg(0, arg);
                    GenerateHook.SetArg(1, var_geometry);
                    script.VM.Query = GenerateHook.Compile();
                    script.VM.Run();

                    if (!script.VM.TryPopSolution(out sol)
                    || sol.Substitutions[var_geometry] is not List geometry)
                    {
                        Throw("Invalid geometry.");
                        // TODO: Log
                        return;
                    }
                    foreach (var item in geometry.Contents)
                    {
                        if (!ParseEML(item, ctx, prefabs))
                            return;
                    }
                })
                .Build(id, map.Size);

        }

        void Throw(string error)
        {
            throw new InvalidOperationException(error);
        }

        bool ParseEML(ITerm term, FloorGenerationContext ctx, Dictionary<string, Prefab> prefabs)
        {
            var sig = term.GetSignature();
            var fun = sig.Functor.Explain();
            var args = term.GetArguments();
            return fun switch
            {
                "draw_line" => Line(),
                "draw_point" => Point(),
                "draw_rect" => Rect(false),
                "fill_rect" => Rect(true),

                "place_feature" => Feature(),
                "place_prefab" => Prefab(),

                _ => false
            };

            bool Prefab()
            {
                if (args.Length != 3)
                {
                    Throw(Err_ExpectedArity(fun, 1, args.Length));
                    return false;
                }
                if (!args[2].Matches<Coord>(out var l1))
                {
                    Throw(Err_ExpectedType(fun, nameof(Coord)));
                    return false;
                }
                if (!args[1].Matches<PlacePrefabArgs>(out var pfbArgs))
                {
                    Throw(Err_ExpectedType(fun, nameof(PlacePrefabArgs)));
                    return false;
                }
                if (!args[0].Matches<string>(out var prefabName))
                {
                    Throw(Err_ExpectedType(fun, nameof(String)));
                    return false;
                }
                if (!prefabs.TryGetValue(prefabName, out var prefab))
                {
                    Throw(Err_DuplicateDefinition(prefabName));
                    return false;
                }
                Coord pos = l1;
                for (int i = 0; i < prefab.Canvas.Length; i++)
                {
                    if (prefab.Canvas[i] is not null)
                    {
                        foreach (var eml in prefab.Canvas[i])
                        {
                            var arg = eml.Concat(TermMarshall.ToTerm(pos));
                            if (!ParseEML(arg, ctx, prefabs))
                                return false;
                        }
                    }
                    if (i % prefab.Size.X == prefab.Size.X - 1 && i > 0)
                        pos = new(l1.X, pos.Y + 1);
                    else pos += Coord.PositiveX;
                }
                return true;
            }

            bool Feature()
            {
                if (args.Length != 2)
                {
                    Throw(Err_ExpectedArity(fun, 1, args.Length));
                    return false;
                }
                if (!args[1].Matches<Coord>(out var l1))
                {
                    Throw(Err_ExpectedType(fun, nameof(Coord)));
                    return false;
                }
                if (!args[0].Matches<FeatureName>(out var t))
                {
                    Throw(Err_ExpectedType(fun, nameof(FeatureName)));
                    return false;
                }
                return ctx.TryAddFeature(t.ToString(), l1, e => t switch
                {
                    FeatureName.Door => e.Feature_Door(),
                    FeatureName.Chest => e.Feature_Chest(),
                    _ => throw new NotSupportedException()
                });
            }

            bool Point()
            {
                if (args.Length != 2)
                {
                    Throw(Err_ExpectedArity(fun, 2, args.Length));
                    return false;
                }
                if (!args[1].Matches<Coord>(out var l1))
                {
                    Throw(Err_ExpectedType(fun, nameof(Coord)));
                    return false;
                }
                if (!args[0].Matches<TileName>(out var t))
                {
                    Throw(Err_ExpectedType(fun, nameof(TileName)));
                    return false;
                }
                ctx.Draw(l1, c => new(t, c));
                return true;
            }

            bool Line()
            {
                if (args.Length != 3)
                {
                    Throw(Err_ExpectedArity(fun, 3, args.Length));
                    return false;
                }
                if (!args[2].Matches<Coord>(out var l1))
                {
                    Throw(Err_ExpectedType(fun, nameof(Coord)));
                    return false;
                }
                if (!args[1].Matches<Coord>(out var l2))
                {
                    Throw(Err_ExpectedType(fun, nameof(Coord)));
                    return false;
                }
                if (!args[0].Matches<TileName>(out var t))
                {
                    Throw(Err_ExpectedType(fun, nameof(TileName)));
                    return false;
                }
                ctx.DrawLine(l1, l2, c => new(t, c));
                return true;
            }

            bool Rect(bool fill)
            {
                if (args.Length != 3)
                {
                    Throw(Err_ExpectedArity(fun, 3, args.Length));
                    return false;
                }
                if (!args[2].Matches<Coord>(out var l1))
                {
                    Throw(Err_ExpectedType(fun, nameof(Coord)));
                    return false;
                }
                if (!args[1].Matches<Coord>(out var l2))
                {
                    Throw(Err_ExpectedType(fun, nameof(Coord)));
                    return false;
                }
                if (!args[0].Matches<TileName>(out var t))
                {
                    Throw(Err_ExpectedType(fun, nameof(TileName)));
                    return false;
                }
                if (fill)
                    ctx.FillBox(l1, l2 - l1 + Coord.PositiveOne, c => new(t, c));
                else
                    ctx.DrawBox(l1, l2 - l1 + Coord.PositiveOne, c => new(t, c));
                return true;
            }

            string Err_ExpectedArity(string fun, int expArity, int actArity) => $"Expected {fun}/{expArity}, found {fun}/{actArity}";
            string Err_ExpectedType(string fun, string type) => $"Expected {type}, found {fun}";
            string Err_DuplicateDefinition(string type) => $"Duplicate definition for prefab {type}";
        }
    }
}
