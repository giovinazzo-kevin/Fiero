using SFML.System;
using System;

namespace Fiero.Core
{
    public sealed class Buffer
    {
        private short[] _buffer;
        private float _bufferLengthSeconds;
        private int _index;

        public readonly int SampleRate;

        public Time Duration {
            get => Time.FromSeconds(_bufferLengthSeconds);
            set {
                _bufferLengthSeconds = value.AsSeconds();
                _buffer = new short[(int)(SampleRate * _bufferLengthSeconds)];
            }
        }

        public int DurationInSamples => _buffer.Length;
        public int AvailableDurationInSamples => _buffer.Length - _index;
        public int UsedDurationInSamples => _index;
        public bool Full => _index >= _buffer.Length;

        public Buffer(int sampleRate)
        {
            SampleRate = sampleRate;
            Duration = Time.FromMilliseconds(20);
        }

        public bool Store(short sample)
        {
            if (Full)
                return false;
            _buffer[_index++] = sample;
            return !Full;
        }

        public int Read(int n, out short[] samples)
        {
            var toRead = Math.Clamp(n, 0, _buffer.Length);
            samples = new short[toRead];
            for (int i = 0; i < toRead; i++) {
                samples[i] = _buffer[i];
            }
            Array.ConstrainedCopy(_buffer, toRead, _buffer, 0, _buffer.Length - toRead);
            _index -= toRead;
            return toRead;
        } 
    }
}
