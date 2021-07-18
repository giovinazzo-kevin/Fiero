using SFML.Graphics;
using System;
using System.Linq;

namespace Fiero.Business
{
    public partial class Animation
    {
        public static Animation ExpandingRing(
            int radius, 
            string sprite = "Skull", 
            TextureName texture = TextureName.Atlas, 
            Color? tint = null,
            TimeSpan? frameDuration = null
        ) => new (
            Enumerable.Range(1, radius)
                .Select(i => new AnimationFrame(frameDuration ?? TimeSpan.FromMilliseconds(8), CoordEnumerable.Circle(i)
                    .Select(p => new AnimationSprite(texture, sprite, tint ?? Color.White, p))
                    .ToArray()))
                .ToArray()
        );
    }
}
