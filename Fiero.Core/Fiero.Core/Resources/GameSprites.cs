using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fiero.Core
{
    public class GameSprites<TTextures>
        where TTextures : struct, Enum
    {
        public class SpritesheetBuilder
        {
            protected readonly TTextures Key;
            protected readonly GameTextures<TTextures> Textures;
            protected readonly Dictionary<string, Sprite> Sprites;

            internal SpritesheetBuilder(GameTextures<TTextures> textures, TTextures key)
            {
                Key = key;
                Textures = textures;
                Sprites = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
            }

            public SpritesheetBuilder WithSprite(string name, Func<Texture, Sprite> build)
            {
                if(Sprites.ContainsKey(name)) {
                    throw new InvalidOperationException($"A spritesheet named {name} already exists");
                }
                Sprites[name] = build(Textures.Get(Key));
                return this;
            }

            internal Dictionary<string, Sprite> Build() => Sprites;
        }

        protected readonly GameTextures<TTextures> Textures;
        protected readonly Dictionary<TTextures, Dictionary<string, Sprite>> Sprites;

        public GameSprites(GameTextures<TTextures> textures)
        {
            Textures = textures;
            Sprites = new Dictionary<TTextures, Dictionary<string, Sprite>>();
        }

        public void AddSpritesheet(TTextures texture, Action<SpritesheetBuilder> build)
        {
            if(Sprites.ContainsKey(texture)) {
                throw new InvalidOperationException($"A spritesheet for texture {texture} already exists");
            }
            var builder = new SpritesheetBuilder(Textures, texture);
            build(builder);
            Sprites[texture] = builder.Build();
        }

        public async Task LoadJsonAsync(TTextures texture, string fileName)
        {
            if (!File.Exists(fileName)) {
                throw new FileNotFoundException(fileName);
            }
            using var fs = new FileStream(fileName, FileMode.Open);
            var dict = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(fs);
            AddSpritesheet(texture, builder => {
                foreach (var kv in dict) {
                    var rect = kv.Value.Split(' ')
                        .Select(x => Int32.TryParse(x.Trim(), out var i) ? i : -1)
                        .ToArray();
                    if(rect.Length != 4) {
                        // TODO: log warning
                        continue;
                    }
                    builder.WithSprite(kv.Key, tex => new Sprite(tex, new(rect[0], rect[1], rect[2], rect[3])));
                }
            });
        }

        public bool TryGet(TTextures texture, string key, out Sprite sprite)
        {
            sprite = default;
            if (!Sprites.TryGetValue(texture, out var dict)) {
                throw new InvalidOperationException($"A spritesheet for texture {texture} does not exist");
            }
            if (!dict.TryGetValue(key, out sprite)) {
                return false;
            }
            return true;
        }

        public Sprite Get(TTextures texture, string key) => TryGet(texture, key, out var s) ? s : null; 
    }
}
