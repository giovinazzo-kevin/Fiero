﻿using Fiero.Core;
using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class AiComponent : EcsComponent
    {
        public LinkedList<MapCell> Path { get; set; }
        public Func<IAction> Goal { get; set; }

        public PhysicalEntity Target { get; set; }
        public List<Func<Item, bool>> LikedItems { get; set; } = new();
        public List<Func<Item, bool>> DislikedItems { get; set; } = new();
    }
}
