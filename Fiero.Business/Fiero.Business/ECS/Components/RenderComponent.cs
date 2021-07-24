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
        protected readonly GameSprites<TextureName> Sprites;

        public RenderComponent(GameSprites<TextureName> sprites)
        {
            Sprites = sprites;
        }

        private string _spriteName;
        public string SpriteName {
            get => _spriteName;
            set {
                if(!Sprites.TryGet(TextureName.Atlas, value, out var sprite)) {
                    throw new ArgumentException($"A sprite named {value} was not declared in the atlas");
                }
                _spriteName = value;
                Sprite = sprite;
            }
        }
        public Sprite Sprite { get; private set; }
        public bool Hidden { get; set; }
    }
}
