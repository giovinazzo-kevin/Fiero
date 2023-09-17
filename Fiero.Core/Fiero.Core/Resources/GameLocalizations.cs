using System.Text.Json;
using System.Text.RegularExpressions;

namespace Fiero.Core
{
    public class GameLocalizations<TLocales>
        where TLocales : struct, Enum
    {
        private readonly Dictionary<TLocales, JsonElement> _translations;

        public readonly TLocales DefaultCulture;
        public TLocales CurrentCulture { get; set; }

        public GameLocalizations()
        {
            CurrentCulture = DefaultCulture = default;
            _translations = new Dictionary<TLocales, JsonElement>();
        }

        public async Task LoadJsonAsync(TLocales culture, string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }
            using var fs = new FileStream(fileName, FileMode.Open);
            var json = await JsonSerializer.DeserializeAsync<JsonElement>(fs);
            AddLocale(culture, json);
        }

        public void AddLocale(TLocales culture, JsonElement value)
        {
            _translations[culture] = value;
        }

        public bool HasLocale(TLocales culture) => _translations.ContainsKey(culture);

        public string Translate(string message)
        {
            foreach (var match in Regex.Matches(message, "\\$(?<key>.*?)\\$").Cast<Match>())
            {
                var translated = Get(match.Groups["key"].Value);
                message = message.Replace(match.Value, translated);
            }
            return message;
        }

        public string Get(string key)
        {
            if (TryGet<string>(key, out var value))
            {
                return value;
            }
            return key;
        }

        public string[] GetArray(string key)
        {
            if (TryGet<string[]>(key, out var array))
            {
                return array;
            }
            return Array.Empty<string>();
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (_translations.TryGetValue(CurrentCulture, out var dict))
            {
                var keyParts = key.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                value = (T)GetInner(dict, keyParts[0], keyParts[1..]);
                return true;
            }
            value = default;
            return false;

            object GetInner(JsonElement outer, string key, params string[] rest)
            {
                if (TryGetIndex(key, out var index))
                {
                    return GetInner(outer, Regex.Replace(key, @"\[(\d+)\]", String.Empty), rest.Prepend($"[{index}]").ToArray());
                }
                if (outer.TryGetProperty(key, out var inner))
                {
                    return Switch(inner, rest);
                }
                return key;

                object Switch(JsonElement inner, params string[] rest)
                {
                    if (inner.ValueKind == JsonValueKind.Object)
                    {
                        if (rest.Length == 0)
                        {
                            return inner;
                        }
                        return GetInner(inner, rest[0], rest[1..]);
                    }
                    else if (inner.ValueKind == JsonValueKind.Array)
                    {
                        if (rest.Length == 0)
                        {
                            return inner.EnumerateArray()
                                .Select(e => e.GetString())
                                .ToArray();
                        }
                        if (!TryGetIndex(rest[0], out var index))
                        {
                            throw new ArgumentException(nameof(rest));
                        }
                        if ("__length".Equals(rest[0], StringComparison.OrdinalIgnoreCase))
                        {
                            return inner.GetArrayLength();
                        }
                        var elem = inner.EnumerateArray().ElementAt(index);
                        return Switch(elem, rest[1..]);
                    }
                    else
                    {
                        return inner.GetString();
                    }
                }
            }

            bool TryGetIndex(string key, out int index)
            {
                if (Regex.Match(key, @".*?\[(\d+)\]") is { Groups: var groups, Success: true })
                {
                    index = Int32.Parse(groups[1].Value);
                    return true;
                }
                index = -1;
                return false;
            }
        }
    }
}
