using SFML.Graphics;
using System.Text.Json;

namespace Fiero.Core
{
    [SingletonDependency]
    public class GameSprites
    {

        public class SpritesheetBuilder
        {
            protected readonly string Key;
            protected readonly GameTextures Textures;
            protected readonly Dictionary<string, HashSet<Sprite>> Sprites;

            internal SpritesheetBuilder(GameTextures textures, string key)
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

        protected readonly GameColors Colors;
        protected readonly GameTextures Textures;
        protected readonly Dictionary<string, Dictionary<string, HashSet<Sprite>>> Sprites;
        protected readonly Dictionary<string, Dictionary<OrderedPair<string, string>, Sprite>> ProceduralSprites;

        public GameSprites(GameTextures textures, GameColors colors)
        {
            Colors = colors;
            Textures = textures;
            Sprites = new();
            ProceduralSprites = new();
        }

        public void AddSpritesheet(string texture, Action<SpritesheetBuilder> build)
        {
            if (Sprites.ContainsKey(texture))
            {
                throw new InvalidOperationException($"A spritesheet for texture {texture} already exists");
            }
            var builder = new SpritesheetBuilder(Textures, texture);
            build(builder);
            Sprites[texture] = builder.Build();
        }

        public void BuildIndex(string atlas, Coord tileSize)
        {
            AddSpritesheet(atlas, builder =>
            {
                var size = Textures.Get(atlas).Size;
                var w = size.X / tileSize.X;
                for (int x = 0; x < w; x++)
                    for (int y = 0; y < size.Y / tileSize.Y; y++)
                        builder.WithSprite(
                            (y * w + x).ToString(),
                            tex => new Sprite(tex, new(x * tileSize.X, y * tileSize.Y, tileSize.X, tileSize.Y)));
            });
        }

        public async Task LoadJsonAsync(string texture, string fileName)
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

        public bool TryGet(string texture, string key, string color, out Sprite sprite, int? rngSeed = null)
        {
            sprite = default;
            if (key is null)
                return false;
            if (!ProceduralSprites.TryGetValue(texture, out var procDict))
            {
                ProceduralSprites[texture] = procDict = new();
            }
            var procKey = new OrderedPair<string, string>(key, color);
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
            var rng = rngSeed is { } seed ? Rng.SeededRandom(seed) : Rng.Random;
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
                    FillColor = color == null ? Color.White : Colors.Get(color),
                    OutlineThickness = 0
                };
                renderTarget.Clear(Color.Transparent);
                renderTarget.Draw(mask);
                renderTarget.Draw(shape, new(BlendMode.Multiply));
                renderTarget.Display();
                using var coloredMaskImage = renderTarget.Texture.CopyToImage();
                using var coloredMaskTex = new Texture(coloredMaskImage);
                using var coloredMaskSprite = new Sprite(coloredMaskTex);
                renderTarget.Clear(Color.Transparent);
                renderTarget.Draw(sprite);
                renderTarget.Draw(coloredMaskSprite);
                renderTarget.Display();
                using var image = renderTarget.Texture.CopyToImage();
                var tex = new Texture(image);
                Textures.StoreProceduralTexture(tex);
                procDict[procKey] = sprite = new(tex, new(0, 0, sprite.TextureRect.Width, sprite.TextureRect.Height));
                return true;
            }
            sprite.Color = color == null ? Color.White : Colors.Get(color);
            return true;
        }

        public Sprite Get(string texture, string sprite, string color, int? rngSeed = null) => TryGet(texture, sprite, color, out var s, rngSeed) ? s : null;

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
