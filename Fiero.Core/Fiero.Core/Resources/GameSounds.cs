using SFML.Audio;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Fiero.Core
{

    public class GameSounds<TSounds>
        where TSounds : struct, Enum
    {
        protected readonly Dictionary<TSounds, SoundBuffer> Buffers;
        protected readonly List<Sound> Sounds;
        protected readonly OffButton OffButton;
             
        public GameSounds(OffButton off)
        {
            Buffers = new Dictionary<TSounds, SoundBuffer>();
            Sounds = new List<Sound>();
            OffButton = off;
        }

        public void Initialize()
        {
            _ = Task.Run(MonitorSounds);
        }

        protected virtual async Task MonitorSounds()
        {
            while(!OffButton.Token.IsCancellationRequested) {
                for (int i = Sounds.Count - 1; i >= 0; i--) {
                    var sound = Sounds[i];
                    if(sound.Status == SoundStatus.Stopped) {
                        Sounds.RemoveAt(i);
                        sound.Dispose();
                    }
                }
                await Task.Delay(250);
            }
        }

        public void Add(TSounds key, SoundBuffer value) => Buffers[key] = value;
        public Sound Get(TSounds key, Coord? pos = null, bool relativeToListener = false)
        {
            if(Buffers.GetValueOrDefault(key) is { } buf) {
                var sound = new Sound(buf);
                if (pos is { } p) {
                    sound.Position = new(p.X, p.Y, 0);
                }
                sound.RelativeToListener = relativeToListener;
                Sounds.Add(sound);
                return sound;
            }
            return null;
        }
    } 
}
