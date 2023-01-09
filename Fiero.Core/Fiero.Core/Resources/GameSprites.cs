using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fiero.Core
{
    public class GameSprites<TTextures, TColors>
        where TTextures : struct, Enum
        where TColors : struct, Enum
    {

        public class SpritesheetBuilder
        {
            protected readonly TTextures Key;
            protected readonly GameTextures<TTextures> Textures;
            protected readonly Dictionary<string, HashSet<Sprite>> Sprites;

            internal SpritesheetBuilder(GameTextures<TTextures> textures, TTextures key)
            {
                Key = key;
                Textures = textures;
                Sprites = new Dictionary<string, HashSet<Sprite>>(StringComparer.OrdinalIgnoreCase);
            }

            public SpritesheetBuilder WithSprite(string name, Func<Texture, Sprite> build)
            {
                if (!Sprites.TryGetValue(name, out var hash))
                {
                    hash = Sprites[name] = new();
                }
                hash.Add(build(Textures.Get(Key)));
                return this;
            }

            internal Dictionary<string, HashSet<Sprite>> Build() => Sprites;
        }

        protected readonly GameColors<TColors> Colors;
        protected readonly GameTextures<TTextures> Textures;
        protected readonly Dictionary<TTextures, Dictionary<string, HashSet<Sprite>>> Sprites;
        protected readonly Dictionary<TTextures, Dictionary<OrderedPair<string, TColors>, Sprite>> ProceduralSprites;

        public GameSprites(GameTextures<TTextures> textures, GameColors<TColors> colors)
        {
            Colors = colors;
            Textures = textures;
            Sprites = new();
            ProceduralSprites = new();
        }

        public void AddSpritesheet(TTextures texture, Action<SpritesheetBuilder> build)
        {
            if (Sprites.ContainsKey(texture))
            {
                throw new InvalidOperationException($"A spritesheet for texture {texture} already exists");
            }
            var builder = new SpritesheetBuilder(Textures, texture);
            build(builder);
            Sprites[texture] = builder.Build();
        }

        public async Task LoadJsonAsync(TTextures texture, string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }
            using var fs = new FileStream(fileName, FileMode.Open);
            var dict = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(fs);
            AddSpritesheet(texture, builder =>
            {
                foreach (var kv in dict)
                {
                    var rect = kv.Value.Split(' ')
                        .Select(x => Int32.TryParse(x.Trim(), out var i) ? i : -1)
                        .ToArray();
                    if (rect.Length % 4 != 0)
                    {
                        // TODO: log warning
                        continue;
                    }
                    for (int i = 0; i < rect.Length / 4; i++)
                    {
                        builder.WithSprite(kv.Key, tex => new Sprite(tex,
                            new(rect[i * 4], rect[i * 4 + 1], rect[i * 4 + 2], rect[i * 4 + 3])))
                        ;
                    }
                }
            });
        }

        public bool TryGet(TTextures texture, string key, TColors color, out Sprite sprite, int? rngSeed = null)
        {
            sprite = default;
            if (key is null)
                return false;
            if (!ProceduralSprites.TryGetValue(texture, out var procDict))
            {
                ProceduralSprites[texture] = procDict = new();
            }
            var procKey = new OrderedPair<string, TColors>(key, color);
            if (procDict.TryGetValue(procKey, out sprite))
            {
                return true;
            }
            if (!Sprites.TryGetValue(texture, out var dict))
            {
                throw new InvalidOperationException($"A spritesheet for texture {texture} does not exist");
            }
            if (!dict.TryGetValue(key, out var sprites))
            {
                return false;
            }
            var rng = rngSeed is { } seed ? Rng.Seeded(seed) : new Random();
            sprite = sprites.Shuffle(rng).First();
            if (dict.TryGetValue($"{key}_Mask", out var masks))
            {
                var mask = masks.Shuffle(rng).First();
                sprite.Color = Color.White;
                mask.Color = Color.White;

                var renderTarget = Textures.GetScratchTexture();
                while (!renderTarget.SetActive(true))
                {
                    continue;
                }

                using var shape = new RectangleShape(sprite.TextureRect.Size())
                {
                    FillColor = Colors.Get(color),
                    OutlineThickness = 0
                };
                renderTarget.Clear(Color.Transparent);
                renderTarget.Draw(mask);
                renderTarget.Draw(shape, new(BlendMode.Multiply));
                renderTarget.Draw(sprite);
                renderTarget.Display();
                using var image = renderTarget.Texture.CopyToImage();
                // image.SaveToFile(@"E:\Repos\Fiero\Fiero.Business\Fiero.Business\Resources\Textures\hmm.png");
                var tex = new Texture(image);
                Textures.StoreProceduralTexture(tex);
                procDict[procKey] = sprite = new(tex, new(0, 0, sprite.TextureRect.Width, sprite.TextureRect.Height));
                return true;
            }
            sprite.Color = Colors.Get(color);
            return true;
        }

        public Sprite Get(TTextures texture, string key, TColors color) => TryGet(texture, key, color, out var s) ? s : null;

        public void ClearProceduralSprites()
        {
            foreach (var sprite in ProceduralSprites.Values.SelectMany(v => v.Values))
            {
                sprite.Dispose();
            }
            ProceduralSprites.Clear();
        }
    }
}
