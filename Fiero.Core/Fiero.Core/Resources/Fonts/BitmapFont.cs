using SFML.Graphics;

namespace Fiero.Core
{
    public class BitmapFont
    {
        protected readonly Sprite[] Sprites;
        public readonly Coord Size;

        public BitmapFont(Coord size, Sprite[] sprites)
        {
            Sprites = sprites;
            Size = size;
        }

        public Sprite Write(char c) => new(Sprites[c]);
        public IEnumerable<Sprite> Write(string s) => Write(Encoding.GetEncoding(437).GetBytes(s));
        public IEnumerable<Sprite> Write(byte[] s) => s
            .Select((c, i) => new Sprite(Sprites[c]) { Position = new Coord(i, 0) * Size });
    }
}
