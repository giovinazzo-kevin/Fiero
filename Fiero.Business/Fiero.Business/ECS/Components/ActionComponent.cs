using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Fiero.Business
{

    public class ActionComponent : EcsComponent
    {
        private readonly GameEntities _entities;

        public ActionComponent(GameEntities entities)
        {
            _entities = entities;
            ActionProvider = _ => new MoveRelativeAction(new());
        }

        public IAction LastAction { get; set; }
        public Func<Actor, IAction> ActionProvider { get; set; }
        public IAction GetAction() => ActionProvider(_entities.GetProxy<Actor>(EntityId));

    }
}
