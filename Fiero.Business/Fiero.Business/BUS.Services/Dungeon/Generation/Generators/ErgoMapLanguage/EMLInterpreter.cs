using Ergo.Interpreter.Libraries;
using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using System.Collections.Immutable;

namespace Fiero.Business
{
    /// <summary>
    /// Interpreter for the EML DSL used to create maps.
    /// </summary>
    public sealed class EMLInterpreter
    {
        #region Types
        [Term(Marshalling = TermMarshalling.Named)]
        public readonly record struct Step(FloorId FloorId, Coord Position, Coord Size);
        [Term(Marshalling = TermMarshalling.Named)]
        public readonly record struct PlacePrefabArgs(
            bool MirrorY, bool MirrorX, int Rotate, bool Randomize
        );
        [Term(Marshalling = TermMarshalling.Named)]
        public readonly record struct Prefab(
            string Name, Coord Size, Coord Offset, string Group, float Weight, int Layer, ITerm[][][] Canvas
        );

        public delegate bool EML(ErgoVM vm, FloorGenerationContext ctx);
        #endregion
        #region Hooks
        public static readonly Hook GenerateHook = new(new(new("generate"), 2, FieroLib.Modules.Map, default));
        public static readonly Hook GetPrefabHook = new(new(new("get_prefab"), 2, FieroLib.Modules.Map, default));
        /// <summary>
        /// Calls map:generate/2 and aggregates a list of EML generation steps.
        /// </summary>
        public static EML GenerateMap(ErgoVM vm, Step arg)
        {
            var var_geometry = new Variable("Geometry");
            GenerateHook.SetArg(0, TermMarshall.ToTerm(arg));
            GenerateHook.SetArg(1, var_geometry);
            vm.Query = GenerateHook.Compile();
            vm.Run();
            if (!vm.TryPopSolution(out var sol)
            || sol.Substitutions[var_geometry] is not List geometry)
            {
                vm.Throw(ErgoVM.ErrorType.Custom, "Invalid geometry.");
                return EML_NoOp(default);
            }
            return InterpretEML(geometry);
        }
        /// <summary>
        /// Calls map:get_prefab/2 and yields all matching prefabs.
        /// NOTE: prefab rules are evaluated each time this is called.
        /// </summary>
        public static IEnumerable<Prefab> GetPrefabs(ErgoVM vm, string name)
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
                || prefab.Canvas.Any(layer => prefab.Size.X * prefab.Size.Y != layer.Length))
                {
                    vm.Throw(ErgoVM.ErrorType.Custom, "Invalid prefab.");
                    yield break;
                }
                yield return prefab;
            }
        }
        #endregion
        #region EML Functions
        /// <summary>
        /// Does nothing.
        /// </summary>
        private static EML EML_Unknown(string name) => (vm, __) => { vm.Throw(ErgoVM.ErrorType.UndefinedPredicate, name); return false; };
        public const string EML_NoOp_Name = "noop";
        /// <summary>
        /// Does nothing.
        /// </summary>
        private static EML EML_NoOp(ImmutableArray<ITerm> _) => (_, __) => true;
        public const string EML_IfTile_Name = "if_tile";
        /// <summary>
        /// If Tile arg0 is at Coord arg2, executes the EML in arg1 at Coord arg2.
        /// </summary>
        private static EML EML_IfTile(ImmutableArray<ITerm> args) => (vm, ctx) =>
        {
            if (args.Length != 3)
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedNArgumentsGotM, 3, args.Length);
                return false;
            }
            if (!args[0].Matches<TileName>(out var tile))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Tile, args[0].Explain());
                return false;
            }
            if (!args[2].Matches<Coord>(out var l1))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Coord, args[2].Explain());
                return false;
            }
            if (!ctx.TryGetTile(l1, out var actualTile) || actualTile.Name != tile)
                return true;
            var arg = args[1] is List lst
                ? new List(lst.Contents.Select(x => x.Concat(args[2])))
                : args[1].Concat(args[2]);
            return InterpretEML(arg)(vm, ctx);
        };
        public const string EML_Chance_Name = "chance";
        /// <summary>
        /// If Float chance at arg0 succeeds, executes the EML in arg1 at Coord arg2.
        /// </summary>
        private static EML EML_Chance(ImmutableArray<ITerm> args) => (vm, ctx) =>
        {
            if (args.Length != 3)
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedNArgumentsGotM, 3, args.Length);
                return false;
            }
            if (!args[0].Matches<float>(out var chance))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Number, args[0].Explain());
                return false;
            }
            if (!Rng.Random.NChancesIn(chance, 1))
                return true;
            var arg = args[1] is List lst
                ? new List(lst.Contents.Select(x => x.Concat(args[2])))
                : args[1].Concat(args[2]);
            return InterpretEML(arg)(vm, ctx);
        };
        public const string EML_DrawLine_Name = "draw_line";
        /// <summary>
        /// Draws a line with Tile arg0 from Coord arg1 to Coord arg2.
        /// </summary>
        private static EML EML_DrawLine(ImmutableArray<ITerm> args) => (vm, ctx) =>
        {
            if (args.Length != 3)
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedNArgumentsGotM, 3, args.Length);
                return false;
            }
            if (!args[2].Matches<Coord>(out var l1))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Coord, args[2].Explain());
                return false;
            }
            if (!args[1].Matches<Coord>(out var l2))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Coord, args[1].Explain());
                return false;
            }
            if (!args[0].Matches<TileName>(out var t))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(TileName), args[0].Explain());
                return false;
            }
            ctx.DrawLine(l1, l2, c => new(t, c));
            return true;
        };
        public const string EML_DrawPoint_Name = "draw_point";
        /// <summary>
        /// Draws a point with Tile arg0 at Coord arg1.
        /// </summary>
        private static EML EML_DrawPoint(ImmutableArray<ITerm> args) => (vm, ctx) =>
        {
            if (args.Length != 2)
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedNArgumentsGotM, 2, args.Length);
                return false;
            }
            if (!args[1].Matches<Coord>(out var l1))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Coord, args[1].Explain());
                return false;
            }
            if (!args[0].Matches<TileName>(out var t))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(TileName), args[0].Explain());
                return false;
            }
            ctx.Draw(l1, c => new(t, c));
            return true;
        };
        public const string EML_DrawRect_Name = "draw_rect";
        private static EML EML_DrawOrFillRect(ImmutableArray<ITerm> args, bool fill) => (vm, ctx) =>
        {
            if (args.Length != 3)
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedNArgumentsGotM, 3, args.Length);
                return false;
            }
            if (!args[2].Matches<Coord>(out var l1))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Coord, args[2].Explain());
                return false;
            }
            if (!args[1].Matches<Coord>(out var l2))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Coord, args[1].Explain());
                return false;
            }
            if (!args[0].Matches<TileName>(out var t))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(TileName), args[0].Explain());
                return false;
            }
            if (fill)
                ctx.FillBox(l1, l2 - l1 + Coord.PositiveOne, c => new(t, c));
            else
                ctx.DrawBox(l1, l2 - l1 + Coord.PositiveOne, c => new(t, c));
            return true;
        };
        /// <summary>
        /// Draws a rect with Tile arg0 of size Coord arg1 at Coord arg2.
        /// </summary>
        private static EML EML_DrawRect(ImmutableArray<ITerm> args)
            => EML_DrawOrFillRect(args, fill: false);
        public const string EML_FillRect_Name = "fill_rect";
        /// <summary>
        /// Fills a rect with Tile arg0 of size Coord arg1 at Coord arg2.
        /// </summary>
        private static EML EML_FillRect(ImmutableArray<ITerm> args)
            => EML_DrawOrFillRect(args, fill: true);
        public const string EML_PlaceFeature_Name = "place_feature";
        /// <summary>
        /// Places feature with Name arg0 at Coord arg1.
        /// </summary>
        private static EML EML_PlaceFeature(ImmutableArray<ITerm> args) => (vm, ctx) =>
        {
            if (args.Length != 2)
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedNArgumentsGotM, 2, args.Length);
                return false;
            }
            if (!args[1].Matches<Coord>(out var l1))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Coord, args[1].Explain());
                return false;
            }
            var featureArgs = new Dict(WellKnown.Literals.Discard);
            if (!args[0].Matches<FeatureName>(out var t))
            {
                if (args[0] is not Dict dict || !dict.Functor.TryGetA(out var df)
                    || !df.Matches(out t))
                {
                    vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(FeatureName), args[0].Explain());
                    return false;
                }
                featureArgs = dict;
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
                FeatureName.DoorSecret => e.Feature_SecretDoor(ctx.Theme.WallTile(Coord.Zero).Color ?? ColorName.Gray),
                FeatureName.SpawnPoint => e.MapTrigger(FeatureName.SpawnPoint),
                FeatureName.PrefabAnchor => e.MapTrigger(FeatureName.PrefabAnchor),
                _ => throw new NotSupportedException()
            });
        };
        public const string EML_PlacePrefab_Name = "place_prefab";
        /// <summary>
        /// Places prefab with Name arg0 at Coord arg2, using placement arguments arg1.
        /// </summary>
        private static EML EML_PlacePrefab(ImmutableArray<ITerm> args) => (vm, ctx) =>
        {
            if (args.Length != 3)
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedNArgumentsGotM, 3, args.Length);
                return false;
            }
            if (!args[2].Matches<Coord>(out var l1))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Coord, args[2].Explain());
                return false;
            }
            if (!args[1].Matches<PlacePrefabArgs>(out var pfbArgs))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(PlacePrefabArgs), args[1].Explain());
                return false;
            }
            if (!args[0].Matches<string>(out var prefabName))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(String), args[0].Explain());
                return false;
            }
            var prefabRules = GetPrefabs(vm, prefabName)
                .OrderBy(p => p.Layer)
                .GroupBy(x => x.Group);
            foreach (var group in prefabRules)
            {
                if (group.Key == "ungrouped")
                {
                    if (!group.Select(PlacePrefab).Any(x => !x))
                        return false;
                }
                else
                {
                    var choice = Rng.Random.ChooseWeighted(group
                        .Select(x => new WeightedItem<Prefab>(x, x.Weight / group.Count()))
                        .ToArray());
                    if (!PlacePrefab(choice))
                        return false;
                }
            }
            return true;

            bool PlacePrefab(Prefab prefab)
            {
                Coord pos = l1;
                // Placement is randomized once at the beginning so that all layers remain consistent.
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
                // Canvas is a 4-dimensional array:
                //   1. Layers: each prefab definition can contain multiple layers that are drawn sequentially.
                //   2. Grids: all grids of all layers for a given definition have the same size.
                //   3. Cells: each cell is represented by a list of EML statements to be executed at that location. 
                //   4. EML: the last argument of functions (position) is omitted, as it is curried automatically by place_prefab.
                foreach (var layer in prefab.Canvas)
                {
                    if (!PlaceLayer(layer))
                        return false;
                }
                return true;
                bool PlaceLayer(ITerm[][] layer)
                {
                    pos = l1 + prefab.Offset;
                    // Mirroring and rotation are implemented as index manipulation. No sorting is required.
                    // Mirroring alters the order in which tiles are placed, rotation alters the grid access pattern.
                    if (pfbArgs.MirrorX)
                        pos = new(pos.X, pos.Y + prefab.Size.Y - 1);
                    if (pfbArgs.MirrorY)
                        pos = new(pos.X + prefab.Size.X - 1, pos.Y);
                    // Used to remember where to go back to once the first length is iterated fully.
                    var startX = pos.X;
                    // Get the grid access pattern from the provided rotation (in degrees).
                    var inc = Rotate(pfbArgs.Rotate, prefab.Size, out int i);
                    for (int j = 0; j < layer.Length; i = inc(i), j++)
                    {
                        // Unbound variables can be used to represent empty cells.
                        if (layer[i] is not null)
                        {
                            foreach (var eml in layer[i])
                            {
                                // Curry the current position as the last argument to the current statement
                                var arg = eml.Concat(TermMarshall.ToTerm(pos));
                                if (!InterpretEML(arg)(vm, ctx))
                                    return false;
                            }
                        }
                        // Change the tile placement order to implement mirroring.
                        if (j.Mod(prefab.Size.X) == prefab.Size.X - 1)
                            pos = pfbArgs.MirrorX ? new(startX, pos.Y - 1) : new(startX, pos.Y + 1);
                        else
                            pos += pfbArgs.MirrorY ? Coord.NegativeX : Coord.PositiveX;
                    }
                    return true;

                    static Func<int, int> Rotate(int deg, Coord size, out int i)
                    {
                        i = 0;
                        var inc = (int i) => i + 1;
                        var rot = deg.Mod(360) / 90;
                        var area = size.X * size.Y;
                        switch (rot)
                        {
                            case 0: break;
                            case 1:
                                i = size.X * (size.Y - 1);
                                inc = (int i) =>
                                {
                                    var d = i - size.X;
                                    if (d >= 0)
                                        return d;
                                    return d.Mod(area) + 1;
                                };
                                break;
                            case 2:
                                i = area - 1;
                                inc = (int i) => i - 1;
                                break;
                            case 3:
                                i = size.X - 1;
                                inc = (int i) =>
                                {
                                    var d = i + size.X;
                                    if (d < area)
                                        return d;
                                    return d.Mod(area) - 1;
                                };
                                break;
                        }
                        return inc;
                    }
                }
            }
        };
        #endregion
        public static EML InterpretEML(List lst)
        {
            return (vm, ctx) => lst.Contents.Select(InterpretEML).All(x => x(vm, ctx));
        }
        public static EML InterpretEML(ITerm term)
        {
            if (term is List lst)
                return InterpretEML(lst);
            var (args, fun) = (term.GetArguments(), term.GetFunctor().Select(x => x.Explain()).GetOr(EML_NoOp_Name));
            return fun switch
            {
                EML_NoOp_Name => EML_NoOp(args),
                EML_IfTile_Name => EML_IfTile(args),
                EML_Chance_Name => EML_Chance(args),
                EML_DrawLine_Name => EML_DrawLine(args),
                EML_DrawPoint_Name => EML_DrawPoint(args),
                EML_DrawRect_Name => EML_DrawRect(args),
                EML_FillRect_Name => EML_FillRect(args),
                EML_PlaceFeature_Name => EML_PlaceFeature(args),
                EML_PlacePrefab_Name => EML_PlacePrefab(args),
                var unknown => EML_Unknown($"{unknown}/{args.Length}"),
            };
        }
    }
}
