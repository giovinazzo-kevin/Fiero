using Ergo.Lang;
using Fiero.Core;
using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    public readonly record struct Objective(PhysicalEntity Target, Func<IAction> Goal);

    public class AiComponent : EcsComponent
    {
        [NonTerm]
        public LinkedList<MapCell> Path { get; set; }
        [NonTerm]
        public Stack<Objective> Objectives { get; set; } = new();
        [NonTerm]
        public List<Func<Item, bool>> LikedItems { get; set; } = new();
        [NonTerm]
        public List<Func<Item, bool>> DislikedItems { get; set; } = new();

        public PhysicalEntity Target => Objectives.Count == 0 ? null : Objectives.Peek().Target;
    }
}
