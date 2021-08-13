using Fiero.Core;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fiero.Business
{


    public class RenderComponent : EcsComponent
    {
        public RenderComponent()
        {
        }

        public string SpriteName { get; set; } = "None";
        public TextureName TextureName { get; set; }
        public ColorName Color { get; set; } = ColorName.White;
        public bool Hidden { get; set; }
    }
}
