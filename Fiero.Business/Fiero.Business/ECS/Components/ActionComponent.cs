using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Fiero.Business
{
    public class ActionComponent : Component
    {
        private readonly GameEntities _entities;

        public ActionComponent(GameEntities entities)
        {
            _entities = entities;
            ActionProvider = _ => ActionName.Move;
        }

        public Coord? Direction { get; set; }
        public Actor Target { get; set; }
        public LinkedList<Tile> Path { get; set; }

        public Func<Actor, ActionName> ActionProvider { get; set; }
        public ActionName GetAction() => ActionProvider(_entities.GetProxy<Actor>(EntityId));

    }
}
