using SFML.Audio;
using System;
using System.Collections.Generic;

namespace Fiero.Core
{
    public class GameSounds<TSounds>
        where TSounds : struct, Enum
    {
        protected readonly Dictionary<TSounds, SoundBuffer> Sounds;

        public GameSounds()
        {
            Sounds = new Dictionary<TSounds, SoundBuffer>();
        }

        public void Add(TSounds key, SoundBuffer value) => Sounds[key] = value;
        public Sound Get(TSounds key) => new Sound(Sounds.GetValueOrDefault(key));
    } 
}
