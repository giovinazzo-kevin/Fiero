using SFML.Graphics;

namespace Fiero.Core
{
    //public class SpriteTermConverter<TTextures, TColors>(GameSprites<TTextures, TColors> sprites) : ITermConverter
    //    where TTextures : struct, Enum
    //    where TColors : struct, Enum
    //{
    //    protected readonly record struct SpriteTerm(TTextures Texture, TColors Color, string Sprite, int? RngSeed = null);

    //    public Type Type => typeof(Sprite);
    //    public TermMarshalling Marshalling => TermMarshalling.Named;

    //    public object FromTerm(ITerm t)
    //    {
    //        if (!t.Match(out SpriteTerm st))
    //            throw new NotSupportedException();
    //        return sprites.Get(st.Texture, st.Sprite, st.Color, rngSeed: st.RngSeed);
    //    }

    //    public ITerm ToTerm(object o, Maybe<Atom> overrideFunctor = default, Maybe<TermMarshalling> overrideMarshalling = default, TermMarshallingContext ctx = null)
    //    {
    //        if (o is not Sprite s)
    //            throw new NotSupportedException();
    //    }
    //}

    public static partial class CoreData
    {
        public static class View
        {
            public static readonly GameDatum<Coord> MinWindowSize = new(nameof(View), nameof(MinWindowSize));
            public static readonly GameDatum<Coord> WindowSize = new(nameof(View), nameof(WindowSize));
            public static readonly GameDatum<Color> DefaultForeground = new(nameof(View), nameof(DefaultForeground));
            public static readonly GameDatum<Color> DefaultBackground = new(nameof(View), nameof(DefaultBackground));
            public static readonly GameDatum<Color> DefaultAccent = new(nameof(View), nameof(DefaultAccent));
        }

        public static class Random
        {
            public static readonly GameDatum<int> Seed = new(nameof(Random), nameof(Seed));
        }

    }
}
