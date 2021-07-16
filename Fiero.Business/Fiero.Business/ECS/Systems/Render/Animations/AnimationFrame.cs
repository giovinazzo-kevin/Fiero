using Fiero.Core;
using System;

namespace Fiero.Business
{
    public readonly struct AnimationFrame
    {
        public readonly TextureName Texture;
        public readonly string Sprite;
        public readonly Coord Offset;
        public readonly TimeSpan Duration;

        public AnimationFrame(string sprite, TextureName texture, Coord ofs, TimeSpan dur)
        {
            Sprite = sprite;
            Texture = texture;
            Offset = ofs;
            Duration = dur;
        }
    }
}
