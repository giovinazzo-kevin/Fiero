using Fiero.Core;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace Fiero.Business
{
    public class Actor : Drawable
    {
        [RequiredComponent]
        public ActionComponent Action { get; private set; }
        [RequiredComponent]
        public ActorComponent Properties { get; private set; }
        [RequiredComponent]
        public FactionComponent Faction { get; private set; }
        public LogComponent Log { get; private set; }
    }
}
