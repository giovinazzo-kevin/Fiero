using SFML.Graphics;

namespace Fiero.Core
{
    public class GameTextures<TTextures>
        where TTextures : struct, Enum
    {
        protected RenderTexture ScratchTexture { get; private set; }
        protected readonly List<Texture> ProceduralTextures;
        protected readonly Dictionary<TTextures, Texture> Textures;

        public void CreateScratchTexture(Coord size) => ScratchTexture = new((uint)size.X, (uint)size.Y);
        public RenderTexture GetScratchTexture() => ScratchTexture;
        public void StoreProceduralTexture(Texture tex)
        {
            ProceduralTextures.Add(tex);
        }

        public void ClearProceduralTextures()
        {
            foreach (var tex in ProceduralTextures)
            {
                tex.Dispose();
            }
            ProceduralTextures.Clear();
        }

        public GameTextures()
        {
            Textures = new();
            ProceduralTextures = new();
        }

        public void Add(TTextures key, Texture value) => Textures[key] = value;
        public Texture Get(TTextures key) => Textures.GetValueOrDefault(key);
    }
}
