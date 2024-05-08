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

        public readonly GameScripts<ScriptName> Scripts = scripts;
        public readonly DungeonTheme Theme = DungeonTheme.Default;

        public readonly Hook GenerateHook = new(new(new("generate"), 2, FieroLib.Modules.Map, default));

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
                        if (!ParseEML(item, ctx))
                            return;
                    }
                })
                .Build(id, map.Size);

        }

        void Throw(string error)
        {
        }

        bool ParseEML(ITerm term, FloorGenerationContext ctx)
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

                _ => false
            };

            bool Feature()
            {
                if (args.Length != 2)
                {
                    Throw(Err_ExpectedArity(fun, 1, args.Length));
                    return false;
                }
                if (!args[0].Matches<Coord>(out var l1))
                {
                    Throw(Err_ExpectedType(fun, nameof(Coord)));
                    return false;
                }
                if (!args[1].Matches<FeatureName>(out var t))
                {
                    Throw(Err_ExpectedType(fun, nameof(FeatureName)));
                    return false;
                }
                return ctx.TryAddFeature(t.ToString(), l1, e => t switch
                {
                    FeatureName.Door => e.Feature_Door(),
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
                if (!args[0].Matches<Coord>(out var l1))
                {
                    Throw(Err_ExpectedType(fun, nameof(Coord)));
                    return false;
                }
                if (!args[1].Matches<TileName>(out var t))
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
                if (!args[0].Matches<Coord>(out var l1))
                {
                    Throw(Err_ExpectedType(fun, nameof(Coord)));
                    return false;
                }
                if (!args[1].Matches<Coord>(out var l2))
                {
                    Throw(Err_ExpectedType(fun, nameof(Coord)));
                    return false;
                }
                if (!args[2].Matches<TileName>(out var t))
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
                if (!args[0].Matches<Coord>(out var l1))
                {
                    Throw(Err_ExpectedType(fun, nameof(Coord)));
                    return false;
                }
                if (!args[1].Matches<Coord>(out var l2))
                {
                    Throw(Err_ExpectedType(fun, nameof(Coord)));
                    return false;
                }
                if (!args[2].Matches<TileName>(out var t))
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
        }
    }
}
