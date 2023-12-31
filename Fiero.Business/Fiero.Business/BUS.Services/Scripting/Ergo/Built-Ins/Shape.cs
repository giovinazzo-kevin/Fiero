using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using LightInject;

namespace Fiero.Business;

[SingletonDependency]
public sealed class Shape : BuiltIn
{
    public readonly IServiceFactory Services;

    public Shape(IServiceFactory services)
        : base("", new("shape"), 4, FieroLib.Modules.Fiero)
    {
        Services = services;
    }

    public override ErgoVM.Op Compile() => vm =>
    {
        if (!vm.Arg(0).Matches<ShapeName>(out var shape))
        {
            vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(ShapeName), vm.Arg(0));
            return;
        }
        if (!vm.Arg(1).Matches<Coord>(out var center))
        {
            vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(Coord), vm.Arg(1));
            return;
        }
        var enumerable = Enumerable.Empty<Coord>();
        if (vm.Arg(2).Matches<int>(out var iSize))
        {
            enumerable = shape switch
            {
                ShapeName.Box => Shapes.Box(Coord.Zero, iSize),
                ShapeName.Square => Shapes.Neighborhood(Coord.Zero, iSize),
                ShapeName.SquareSpiral => Shapes.SquareSpiral(Coord.Zero, iSize),
                ShapeName.Disc => Shapes.Disc(Coord.Zero, iSize),
                ShapeName.Circle => Shapes.Circle(Coord.Zero, iSize),
                _ => enumerable
            };
        }
        else if (vm.Arg(2).Matches<Coord>(out var pSize))
        {
            enumerable = shape switch
            {
                ShapeName.Rect => Shapes.Rect(Coord.Zero, pSize),
                ShapeName.Line => Shapes.Line(Coord.Zero, pSize),
                _ => enumerable
            };
        }
        else
        {
            vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Integer, vm.Arg(2));
            return;
        }
        if (!enumerable.Any())
        {
            vm.Fail();
            return;
        }
        foreach (var p in enumerable
            .Select(x => x + center))
        {
            var term = TermMarshall.ToTerm(p, functor: new Atom(nameof(p)), mode: TermMarshalling.Positional);
            vm.SetArg(0, vm.Arg(3));
            vm.SetArg(1, term);
            ErgoVM.Goals.Unify2(vm);
            vm.SuccessToSolution();
        }
    };
}
