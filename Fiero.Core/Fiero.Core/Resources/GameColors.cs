using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fiero.Core
{
    public class GameColors<TColors>
        where TColors : struct, Enum
    {
        protected readonly Dictionary<TColors, Color> Colors;

        public GameColors()
        {
            Colors = new Dictionary<TColors, Color>();
        }

        public Color Get(TColors col) => Colors[col];

        public async Task LoadJsonAsync(string fileName)
        {
            if (!File.Exists(fileName)) {
                throw new FileNotFoundException(fileName);
            }
            var allColors = Enum.GetValues<TColors>();
            using var fs = new FileStream(fileName, FileMode.Open);
            var dict = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(fs);
            foreach (var kv in dict) {
                var rgb = kv.Value.Split(' ')
                    .Select(x => Byte.TryParse(x.Trim(), out var i) ? i : (byte)0)
                    .ToArray();
                if (rgb.Length != 3) {
                    // TODO: log warning
                    continue;
                }
                var key = allColors.Cast<TColors?>()
                    .DefaultIfEmpty(null)
                    .FirstOrDefault(c => c.ToString().Equals(kv.Key, StringComparison.OrdinalIgnoreCase));
                if(key == null) {
                    // TODO: log warning
                    continue;
                }
                Colors[key.Value] = new Color(rgb[0], rgb[1], rgb[2], 255);
            }
        }
    }
}
