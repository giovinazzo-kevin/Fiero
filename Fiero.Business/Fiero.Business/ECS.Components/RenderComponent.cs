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

        public string Sprite { get; set; } = "None";
        public RenderLayerName Layer { get; set; }
        public TextureName Texture { get; set; }
        public ColorName Color { get; set; } = ColorName.White;
        public bool Hidden { get; set; }
    }
}
