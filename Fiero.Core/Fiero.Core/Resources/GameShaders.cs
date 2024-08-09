using SFML.Graphics;

namespace Fiero.Core
{
    [SingletonDependency]
    public class GameShaders
    {
        protected readonly Dictionary<string, Shader> Shaders;

        public GameShaders()
        {
            Shaders = new Dictionary<string, Shader>();
        }
        public void Add(string key, Shader value) => Shaders[key] = value;
        public Shader Get(string key) => Shaders.GetValueOrDefault(key);
    }
}
