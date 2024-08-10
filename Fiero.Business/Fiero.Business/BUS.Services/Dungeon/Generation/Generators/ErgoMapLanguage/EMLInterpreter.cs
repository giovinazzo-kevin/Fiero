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
        public readonly record struct Step(string Name, Coord Size, Coord Position, FloorId FloorId, int Layer, ITerm[][] EML);
        [Term(Marshalling = TermMarshalling.Named)]
        public readonly record struct Prefab(
            string Name, Coord Size, Coord GridSize, Coord Offset, string Group, float Weight, int Layer, ITerm[][][] Canvas, string[] Tags
        );
        [Term(Marshalling = TermMarshalling.Named)]
        public readonly record struct PlacePrefabArgs(
            bool MirrorY, bool MirrorX, int Rotate, bool Randomize, bool CenterX, bool CenterY, Coord Size, string[] Tags
        );

        public delegate bool EML(ErgoVM vm, FloorGenerationContext ctx);
        #endregion
        #region Hooks
        public static readonly Hook GetPrefabHook = new(new(new("get_prefab"), 3, FieroLib.Modules.Map, default));
        public static readonly Hook GetStepHook = new(new(new("get_step"), 3, FieroLib.Modules.Map, default));

        /// <summary>
        /// Calls map:generate/2 and aggregates a list of EML generation steps.
        /// </summary>
        public static EML GenerateMap(Coord size, FloorId id) => (vm, ctx) =>
        {
            var steps = GetSteps(vm, size, id);
            var geometry = new List(steps
                .OrderBy(s => s.Layer)
                .SelectMany(s => s.EML.SelectMany(x => x)));
            foreach (var geom in geometry.Contents)
            {
                if (!InterpretEML(geom)(vm, ctx))
                    return false;
            }
            return true;
        };
        /// <summary>
        /// Calls map:get_prefab/3 and yields all matching prefabs.
        /// NOTE: prefab rules are evaluated each time this is called.
        /// </summary>
        public static IEnumerable<Prefab> GetPrefabs(ErgoVM vm, Maybe<string> name, Maybe<Coord> size = default)
        {
            var var_prefab = new Variable("Prefab");
            var nameArg = name.Reduce(
                some => TermMarshall.ToTerm(some),
                () => new Variable("Name"));
            var sizeArg = size.Reduce(
                some => TermMarshall.ToTerm(some),
                () => WellKnown.Literals.Discard);
            GetPrefabHook.SetArg(0, nameArg);
            GetPrefabHook.SetArg(1, sizeArg);
            GetPrefabHook.SetArg(2, var_prefab);
            vm.Query = GetPrefabHook.Compile();
            foreach (var sol in vm.RunInteractive())
            {
                var item = sol.Substitutions[var_prefab]
                    .Substitute(sol.Substitutions);
                if (!item.Match(out Prefab prefab)
                || prefab.Canvas.Any(layer => prefab.Size.Area() / prefab.GridSize.Area() != layer.Length))
                {
                    vm.Throw(ErgoVM.ErrorType.Custom, "Invalid prefab.");
                    yield break;
                }
                yield return prefab;
            }
        }
        /// <summary>
        /// Calls map:get_steps/4 and yields all matching generation steps.
        /// NOTE: steps are evaluated each time this is called.
        /// </summary>
        public static IEnumerable<Step> GetSteps(ErgoVM vm, Coord size, FloorId id)
        {
            var var_step = new Variable("Step");
            GetStepHook.SetArg(0, TermMarshall.ToTerm(size));
            GetStepHook.SetArg(1, TermMarshall.ToTerm(id));
            GetStepHook.SetArg(2, var_step);
            vm.Query = GetStepHook.Compile();
            foreach (var sol in vm.RunInteractive())
            {
                var item = sol.Substitutions[var_step]
                    .Substitute(sol.Substitutions);
                if (!item.Match(out Step step))
                {
                    vm.Throw(ErgoVM.ErrorType.Custom, "Invalid step.");
                    yield break;
                }
                yield return step;
            }
        }
        public static ITerm[][] ParseContext(FloorGenerationContext ctx)
        {
            var l1 = ctx.Size.X * ctx.Size.Y;
            var layer = new ITerm[l1][];
            for (int i = 0; i < l1; i++)
            {
                var p = new Coord(i % ctx.Size.X, i / ctx.Size.X);
                var stuffHere = new List<ITerm>();
                if (ctx.TryGetTile(p, out var t))
                    stuffHere.Add(new Complex(new(EML_DrawPoint_Name), TermMarshall.ToTerm(t.Name)));
                stuffHere.AddRange(ctx.GetObjectsAt(p)
                    .Where(x => x.Build == null && x.Name == CTX_MapMarker_Name)
                    .Select(x => (ITerm)new Complex(new(EML_PlaceMarker_Name), (Dict)x.Data)));
                if (stuffHere.Count > 0)
                    layer[i] = [.. stuffHere];
            }
            return layer;
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
            if (!args[0].Match<string>(out var tile))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.String, args[0].Explain());
                return false;
            }
            tile = tile.ToCSharpCase();
            if(!TileName._Values.Contains(tile))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Tile, args[0].Explain());
                return false;
            }
            if (!args[2].Match<Coord>(out var l1))
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
            if (!args[0].Match<float>(out var chance))
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
            if (!args[2].Match<Coord>(out var l1))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Coord, args[2].Explain());
                return false;
            }
            if (!args[1].Match<Coord>(out var l2))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Coord, args[1].Explain());
                return false;
            }
            if (!args[0].Match<string>(out var tile))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.String, args[0].Explain());
                return false;
            }
            tile = tile.ToCSharpCase();
            if (!TileName._Values.Contains(tile))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Tile, args[0].Explain());
                return false;
            }
            ctx.DrawLine(l1, l2, c => new(tile, c));
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
            if (!args[1].Match<Coord>(out var l1))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Coord, args[1].Explain());
                return false;
            }
            if (!args[0].Match<string>(out var tile))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.String, args[0].Explain());
                return false;
            }
            tile = tile.ToCSharpCase();
            if (!TileName._Values.Contains(tile))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Tile, args[0].Explain());
                return false;
            }
            ctx.Draw(l1, c => new(tile, c));
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
            if (!args[2].Match<Coord>(out var l1))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Coord, args[2].Explain());
                return false;
            }
            if (!args[1].Match<Coord>(out var size))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Coord, args[1].Explain());
                return false;
            }
            if (!args[0].Match<string>(out var tile))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(String), args[0].Explain());
                return false;
            }
            tile = tile.ToCSharpCase();
            if(!TileName._Values.Contains(tile))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(TileName), args[0].Explain());
                return false;
            }
            if (fill)
                ctx.FillBox(l1, size, c => new(tile, c));
            else
                ctx.DrawBox(l1, size, c => new(tile, c));
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
        public const string EML_PlaceMarker_Name = "place_marker";
        public const string CTX_MapMarker_Name = "MapMarker";
        /// <summary>
        /// Places map marker with Name arg0 and Data arg0 at Coord arg1.
        /// </summary>
        private static EML EML_PlaceMarker(ImmutableArray<ITerm> args) => (vm, ctx) =>
        {
            if (args.Length != 2)
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedNArgumentsGotM, 2, args.Length);
                return false;
            }
            if (!args[1].Match<Coord>(out var l1))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Coord, args[1].Explain());
                return false;
            }
            var t = default(MapMarkerName);
            var markerArgs = new Dict(WellKnown.Literals.Discard);
            if (args[0] is Dict dict && dict.Functor.TryGetA(out var df))
            {
                if (!df.Match(out t))
                {
                    vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(MapMarkerName), args[0].Explain());
                    return false;
                }
                markerArgs = dict;
            }
            else if (!args[0].Match(out t))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(MapMarkerName), args[0].Explain());
                return false;
            }
            var fun = t.ToString();
            ctx.AddMetaObject(CTX_MapMarker_Name, l1, markerArgs.WithFunctor(new Atom(fun.ToErgoCase())));
            return true;
        };
        public const string EML_PlacePrefab_Name = "place_prefab";
        /// <summary>
        /// Places prefab with Name arg0 at Coord arg2, using placement arguments arg1.
        /// </summary>
        private static EML EML_PlacePrefab(ImmutableArray<ITerm> args) => (vm, ctx) =>
        {
            if (args.Length != 4)
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedNArgumentsGotM, 4, args.Length);
                return false;
            }
            if (!args[3].Match<Coord>(out var l1))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Coord, args[3].Explain());
                return false;
            }
            if (!args[2].Match<PlacePrefabArgs>(out var pfbArgs))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(PlacePrefabArgs), args[2].Explain());
                return false;
            }
            var desiredSize = Maybe<Coord>.None;
            if (args[1] is not Variable && args[1].Match<Coord>(out var desiredSize_))
                desiredSize = desiredSize_;
            else if (args[1] is not Variable)
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(Coord), args[1].Explain());
                return false;
            }
            var desiredName = Maybe<string>.None;
            if (args[0].Match<string>(out var desiredName_)
                && desiredName_ != null)
                desiredName = desiredName_;
            else if (args[0] is not Variable)
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(String), args[0].Explain());
                return false;
            }
            var prefabGroups = GetPrefabs(vm, desiredName, desiredSize)
                .Where(x => pfbArgs.Tags == null || pfbArgs.Tags.Intersect(x.Tags).Any())
                .GroupBy(x => x.Name);
            var prefabRules = Rng.Random.Choose(prefabGroups.ToList())
                .OrderBy(x => x.Layer)
                .GroupBy(x => x.Group);
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
            foreach (var group in prefabRules)
            {
                if (group.Key == "ungrouped")
                {
                    if (group
                        .Select(x => !PlacePrefab(x, pfbArgs)).Any(x => x))
                        return false;
                }
                else
                {
                    var choice = Rng.Random.ChooseWeighted(group
                        .Select(x => new WeightedItem<Prefab>(x, x.Weight / group.Count()))
                        .ToArray());
                    if (!PlacePrefab(choice, pfbArgs))
                        return false;
                }
            }
            return true;

            bool PlacePrefab(Prefab prefab, PlacePrefabArgs pfbArgs)
            {
                var p = l1;
                if (pfbArgs.CenterX)
                    p -= new Coord(prefab.Size.X / 2, 0);
                if (pfbArgs.CenterY)
                    p -= new Coord(0, prefab.Size.Y / 2);
                Coord pos = p;
                // Canvas is a 4-dimensional array:
                //   1. Layers: each prefab definition can contain multiple layers that are drawn sequentially.
                //   2. Grids: all grids of all layers for a given definition have the same size.
                //   3. Cells: each cell is represented by a list of EML statements to be executed at that location. 
                //   4. EML: the last argument of functions (position) is omitted, as it is curried automatically by place_prefab.
                foreach (var layer in prefab.Canvas)
                {
                    // Since prefabs can be rotated and mirrored, and since they can contain more prefabs in their definition,
                    // they have to be pre-rendered fully onto a fresh context in a known reference frame (no transformations).
                    if (!Prerender(layer).TryGetValue(out var renderedLayer))
                        return false;
                    if (!PlaceLayer(renderedLayer))
                        return false;
                }
                return true;
                Maybe<ITerm[][]> Prerender(ITerm[][] layer)
                {
                    var pos = Coord.Zero;
                    var freshCtx = ctx.CreateSubContext(prefab.Size);
                    var mod = prefab.Size.X / prefab.GridSize.X;
                    for (int i = 0; i < layer.Length; i++)
                    {
                        if (layer[i] is not null)
                        {
                            foreach (var eml in layer[i])
                            {
                                // Curry the current position as the last argument to the current statement
                                var arg = eml.Concat(TermMarshall.ToTerm(pos));
                                if (!InterpretEML(arg)(vm, freshCtx))
                                    return default;
                            }
                        }
                        if (i.Mod(mod) == mod - 1)
                            pos = new(0, pos.Y + prefab.GridSize.Y);
                        else
                            pos += Coord.PositiveX * prefab.GridSize;
                    }
                    return ParseContext(freshCtx);
                }
                bool PlaceLayer(ITerm[][] layer)
                {
                    pos = p + prefab.Offset; // Used to remember where to go back to once the first length is iterated fully.
                    var size = prefab.Size;
                    // Get the grid access pattern from the provided rotation (in degrees).
                    var inc = Rotate(pfbArgs.Rotate, ref size, out int i);
                    // Mirroring and rotation are implemented as index manipulation. No sorting is required.
                    // Mirroring alters the order in which tiles are placed, rotation alters the grid access pattern.
                    if (pfbArgs.MirrorX)
                        pos = new(pos.X, pos.Y + size.Y - 1);
                    if (pfbArgs.MirrorY)
                        pos = new(pos.X + size.X - 1, pos.Y);
                    var startX = pos.X;
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
                        if (j.Mod(size.X) == size.X - 1)
                            pos = pfbArgs.MirrorX ? new(startX, pos.Y - 1) : new(startX, pos.Y + 1);
                        else
                        {
                            pos += pfbArgs.MirrorY ? Coord.NegativeX : Coord.PositiveX;
                        }
                    }
                    return true;

                    static Func<int, int> Rotate(int deg, ref Coord size, out int i)
                    {
                        i = 0;
                        var inc = (int i) => i + 1;
                        var rot = deg.Mod(360) / 90;
                        var sizecpy = size;
                        var area = sizecpy.X * sizecpy.Y;
                        switch (rot)
                        {
                            case 0: break;
                            case 1:
                                i = sizecpy.X * (sizecpy.Y - 1);
                                inc = (int i) =>
                                {
                                    var d = i - sizecpy.X;
                                    if (d >= 0)
                                        return d;
                                    return d.Mod(area) + 1;
                                };
                                size = new(size.Y, size.X);
                                break;
                            case 2:
                                i = area - 1;
                                inc = (int i) => i - 1;
                                break;
                            case 3:
                                i = sizecpy.X - 1;
                                inc = (int i) =>
                                {
                                    var d = i + sizecpy.X;
                                    if (d < area)
                                        return d;
                                    return d.Mod(area) - 1;
                                };
                                size = new(size.Y, size.X);
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
            //Debug.WriteLine(term.Explain());
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
                EML_PlacePrefab_Name => EML_PlacePrefab(args),
                EML_PlaceMarker_Name => EML_PlaceMarker(args),
                var unknown => EML_Unknown($"{unknown}/{args.Length}"),
            };
        }
    }
}
