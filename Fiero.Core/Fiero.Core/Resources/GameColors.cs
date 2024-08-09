using SFML.Graphics;
using System.IO;
using System.Text.Json;

namespace Fiero.Core
{
    [SingletonDependency]
    public class GameColors
    {
        protected readonly Dictionary<string, Color> Colors;

        public GameColors()
        {
            Colors = new Dictionary<string, Color>();
        }

        public Color Get(string col) => Colors[col];
        public bool TryGet(string col, out Color value) => Colors.TryGetValue(col, out value);

        public async Task LoadJsonAsync(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }
            using var fs = new FileStream(fileName, FileMode.Open);
            var dict = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(fs);
            foreach (var kv in dict)
            {
                var rgb = kv.Value.Split(' ')
                    .Select(x => Byte.TryParse(x.Trim(), out var i) ? i : (byte)0)
                    .ToArray();
                if (rgb.Length < 3)
                {
                    // TODO: log warning
                    continue;
                }
                Colors[kv.Key] = new Color(rgb[0], rgb[1], rgb[2], rgb.Length > 3 ? rgb[3] : (byte)255);
            }
        }
    }
}
