using SFML.Audio;

namespace Fiero.Core
{

    [SingletonDependency]
    public class GameSounds
    {
        protected readonly Dictionary<string, SoundBuffer> Buffers;
        protected readonly List<Sound> Sounds;
        protected readonly OffButton OffButton;

        public GameSounds(OffButton off)
        {
            Buffers = new Dictionary<string, SoundBuffer>();
            Sounds = new List<Sound>();
            OffButton = off;
            _ = Task.Run(MonitorSounds);
        }

        protected virtual async Task MonitorSounds()
        {
            while (!OffButton.Token.IsCancellationRequested)
            {
                for (int i = Sounds.Count - 1; i >= 0; i--)
                {
                    var sound = Sounds[i];
                    if (sound.Status == SoundStatus.Stopped)
                    {
                        Sounds.RemoveAt(i);
                        sound.Dispose();
                    }
                }
                await Task.Delay(60);
            }
        }

        public void Add(string key, SoundBuffer value) => Buffers[key] = value;
        public Sound Get(string key, Coord? pos = null, float vol = 25, float pitch = 1, bool relativeToListener = false)
        {
            if (Buffers.GetValueOrDefault(key) is { } buf)
            {
                var sound = new Sound(buf) { Volume = vol };
                if (pos is { } p)
                {
                    sound.Position = new(p.X, p.Y, 0);
                }
                sound.Pitch = pitch;
                sound.RelativeToListener = relativeToListener;
                Sounds.Add(sound);
                return sound;
            }
            return null;
        }
    }
}
