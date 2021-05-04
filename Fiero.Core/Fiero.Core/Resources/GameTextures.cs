using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fiero.Core
{
    public class GameTextures<TTextures>
        where TTextures : struct, Enum
    {
        protected readonly Dictionary<TTextures, Texture> Textures;

        public GameTextures()
        {
            Textures = new Dictionary<TTextures, Texture>();
        }
        public void Add(TTextures key, Texture value) => Textures[key] = value;
        public Texture Get(TTextures key) => Textures.GetValueOrDefault(key);
    }
}
