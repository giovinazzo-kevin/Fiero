using SFML.Graphics;

namespace Fiero.Core
{
    [SingletonDependency]
    public class GameFonts
    {
        protected readonly Dictionary<string, BitmapFont> Fonts;

        public GameFonts()
        {
            Fonts = new Dictionary<string, BitmapFont>();
        }

        public void Add(string key, Coord fontSize, Texture tex)
        {
            var sprites = new Sprite[256];
            for (int i = 0; i < 256; i++)
            {
                sprites[i] = new(tex, new((i % 16) * fontSize.X, (i / 16) * fontSize.Y, fontSize.X, fontSize.Y));
            }
            Fonts[key] = new(fontSize, sprites);
        }

        public BitmapFont Get(string key) => Fonts.GetValueOrDefault(key);
    }
}
