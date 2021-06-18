using SFML.Graphics;
using System;
using System.Collections.Generic;

namespace Fiero.Core
{
    public class GameShaders<TShaders>
        where TShaders : struct, Enum
    {
        protected readonly Dictionary<TShaders, Shader> Shaders;

        public GameShaders()
        {
            Shaders = new Dictionary<TShaders, Shader>();
        }
        public void Add(TShaders key, Shader value) => Shaders[key] = value;
        public Shader Get(TShaders key) => Shaders.GetValueOrDefault(key);
    }
}
