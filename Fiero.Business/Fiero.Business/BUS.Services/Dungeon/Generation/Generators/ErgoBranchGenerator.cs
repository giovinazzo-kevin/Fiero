using Ergo.Interpreter.Libraries;
using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;

namespace Fiero.Business
{
    [SingletonDependency]
    public class ErgoBranchGenerator(GameScripts<ScriptName> scripts) : IBranchGenerator
    {
        [Term(Marshalling = TermMarshalling.Named)]
        public readonly record struct Step(FloorId FloorId, Coord Position, Coord Size);
        [Term(Marshalling = TermMarshalling.Named)]
        public readonly record struct PlacePrefabArgs(
            bool MirrorY, bool MirrorX, int Rotate, bool Randomize
        );
        [Term(Marshalling = TermMarshalling.Named)]
        public readonly record struct Prefab(
            string Name, Coord Size, Coord Offset, string Group, float Weight, int Layer, ITerm[][] Canvas
        );

        [Term(Marshalling = TermMarshalling.Named)]
        public readonly record struct StairConnection(
            FloorId To
        );

        public readonly GameScripts<ScriptName> Scripts = scripts;
        public readonly DungeonTheme Theme = DungeonTheme.Default;

        public readonly Hook GenerateHook = new(new(new("generate"), 2, FieroLib.Modules.Map, default));
        public readonly Hook GetPrefabHook = new(new(new("get_prefab"), 2, FieroLib.Modules.Map, default));

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
                    var var_geometry = new Variable("Geometry");
                    GenerateHook.SetArg(0, arg);
                    GenerateHook.SetArg(1, var_geometry);
                    script.VM.Query = GenerateHook.Compile();
                    script.VM.Run();

                    if (!script.VM.TryPopSolution(out var sol)
                    || sol.Substitutions[var_geometry] is not List geometry)
                    {
                        Throw("Invalid geometry.");
                        // TODO: Log
                        return;
                    }
                    foreach (var item in geometry.Contents)
                    {
                        if (!ParseEML(item, ctx, script.VM))
                            return;
                    }
                })
                .Build(id, map.Size);

        }

        void Throw(string error)
        {
            throw new InvalidOperationException(error);
        }

        IEnumerable<Prefab> GetPrefabRules(ErgoVM vm, string name)
        {
            var var_prefab = new Variable("Prefab");
            GetPrefabHook.SetArg(0, new Atom(name));
            GetPrefabHook.SetArg(1, var_prefab);
            vm.Query = GetPrefabHook.Compile();
            foreach (var sol in vm.RunInteractive())
            {
                var item = sol.Substitutions[var_prefab]
                    .Substitute(sol.Substitutions);
                if (!item.Matches(out Prefab prefab)
                || prefab.Size.X * prefab.Size.Y != prefab.Canvas.Length)
                {
                    Throw("Invalid prefab.");
                    // TODO: Log
                    yield break;
                }
                yield return prefab;
            }
        }

        bool ParseEML(ITerm term, FloorGenerationContext ctx, ErgoVM vm)
        {
            if (term is List lst)
            {
                foreach (var item in lst.Contents)
                {
                    if (!ParseEML(item, ctx, vm))
                        return false;
                }
                return true;
            }

            var sig = term.GetSignature();
            var fun = sig.Functor.Explain();
            var args = term.GetArguments();
            return fun switch
            {
                "chance" => Chance(),
                "draw_line" => Line(),
                "draw_point" => Point(),
                "draw_rect" => Rect(false),
                "fill_rect" => Rect(true),

                "place_feature" => Feature(),
                "place_prefab" => Prefab(),

                _ => false
            };

            bool Chance()
            {
                if (args.Length != 3)
                {
                    Throw(Err_ExpectedArity(fun, 3, args.Length));
                    return false;
                }
                if (!args[1].Matches<float>(out var chance))
                {
                    Throw(Err_ExpectedType(fun, WellKnown.Types.Number));
                    return false;
                }
                if (!Rng.Random.NChancesIn(chance, 1))
                    return true;
                var arg0 = args[0];
                if (arg0 is List lst)
                    arg0 = new List(lst.Contents.Select(x => x.Concat(args[2])));
                else arg0 = arg0.Concat(args[2]);
                return ParseEML(arg0, ctx, vm);
            }

            bool Prefab()
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
                var prefabRules = GetPrefabRules(vm, prefabName)
                    .OrderBy(p => p.Layer)
                    .GroupBy(x => x.Group)
                    .ToList();
                foreach (var group in prefabRules)
                {
                    if (group.Key == "ungrouped")
                    {
                        foreach (var item in group)
                        {
                            if (!Inner(item))
                                return false;
                        }
                    }
                    else
                    {
                        var choice = Rng.Random.ChooseWeighted(group
                            .Select(x => new WeightedItem<Prefab>(x, x.Weight / group.Count()))
                            .ToArray());
                        if (!Inner(choice))
                            return false;
                    }
                }
                return true;

                bool Inner(Prefab prefab)
                {
                    Coord pos = l1;
                    if (pfbArgs.Randomize)
                    {
                        pfbArgs = pfbArgs with
                        {
                            MirrorX = Rng.Random.NChancesIn(1, 2),
                            MirrorY = Rng.Random.NChancesIn(1, 2),
                            Rotate = Rng.Random.Between(0, 3) * 90,
                            Randomize = false
                        };
                    }
                    if (pfbArgs.MirrorX)
                        pos = new(pos.X, pos.Y + prefab.Size.Y - 1);
                    if (pfbArgs.MirrorY)
                        pos = new(pos.X + prefab.Size.X - 1, pos.Y);
                    var startX = pos.X;

                    var i = 0;
                    var inc = (int i) => i + 1;
                    var rot = pfbArgs.Rotate.Mod(360) / 90;
                    switch (rot)
                    {
                        case 0: break;
                        case 1:
                            i = prefab.Size.X * (prefab.Size.Y - 1);
                            inc = (int i) =>
                            {
                                var d = i - prefab.Size.X;
                                if (d >= 0)
                                    return d;
                                return d.Mod(prefab.Canvas.Length) + 1;
                            };
                            break;
                        case 2:
                            i = prefab.Canvas.Length - 1;
                            inc = (int i) => i - 1;
                            break;
                        case 3:
                            i = prefab.Size.X - 1;
                            inc = (int i) =>
                            {
                                var d = i + prefab.Size.X;
                                if (d < prefab.Canvas.Length)
                                    return d;
                                return d.Mod(prefab.Canvas.Length) - 1;
                            };
                            break;
                    }

                    for (int j = 0; j < prefab.Canvas.Length; i = inc(i), j++)
                    {
                        if (prefab.Canvas[i] is not null)
                        {
                            foreach (var eml in prefab.Canvas[i])
                            {
                                var arg = eml.Concat(TermMarshall.ToTerm(pos));
                                if (!ParseEML(arg, ctx, vm))
                                    return false;
                            }
                        }
                        if (j.Mod(prefab.Size.X) == prefab.Size.X - 1)
                        {
                            if (pfbArgs.MirrorX)
                                pos = new(startX, pos.Y - 1);
                            else
                                pos = new(startX, pos.Y + 1);
                        }
                        else
                        {
                            if (pfbArgs.MirrorY)
                                pos += Coord.NegativeX;
                            else
                                pos += Coord.PositiveX;
                        }

                    }
                    return true;
                }
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
                var featureArgs = new Dict(WellKnown.Literals.Discard);
                if (!args[0].Matches<FeatureName>(out var t))
                {
                    if (args[0] is Dict dict && dict.Functor.TryGetA(out var df)
                        && df.Matches(out t))
                    {
                        featureArgs = dict;
                    }
                    else
                    {
                        Throw(Err_ExpectedType(fun, nameof(FeatureName)));
                        return false;
                    }
                }
                return ctx.TryAddFeature(t.ToString(), l1, e => t switch
                {
                    FeatureName.Door => e.Feature_Door(),
                    FeatureName.Chest => e.Feature_Chest(),
                    FeatureName.Trap => e.Feature_Trap(),
                    FeatureName.Shrine => e.Feature_Shrine(),
                    FeatureName.Statue => e.Feature_Statue(),
                    FeatureName.Downstairs
                        => e.Feature_Downstairs(new(ctx.Id, new(ctx.Id.Branch, ctx.Id.Depth + 1))),
                    FeatureName.Upstairs
                        => e.Feature_Upstairs(new(new(ctx.Id.Branch, ctx.Id.Depth - 1), ctx.Id)),
                    FeatureName.DoorSecret => e.Feature_SecretDoor(Theme.WallTile(Coord.Zero).Color ?? ColorName.Gray),
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
