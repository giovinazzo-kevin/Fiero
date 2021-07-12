using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Fiero.Business
{

    public class ActionComponent : EcsComponent
    {
        public ActionComponent()
        {
        }

        public IAction LastAction { get; set; }
        public ActionProvider ActionProvider { get; set; }

    }
}
