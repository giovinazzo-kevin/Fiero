using SFML.System;
using System;
using System.Threading;

namespace Fiero.Core
{
    public sealed class Buffer
    {
        private readonly ReaderWriterLockSlim Lock = new();

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
            Lock.EnterWriteLock();
            _buffer[_index++] = sample;
            Lock.ExitWriteLock();
            return !Full;
        }

        public int Read(int n, out short[] samples)
        {
            var toRead = Math.Clamp(n, 0, _buffer.Length);
            samples = new short[toRead];
            Lock.EnterReadLock();
            for (int i = 0; i < toRead; i++) {
                samples[i] = _buffer[i];
            }
            Array.ConstrainedCopy(_buffer, toRead, _buffer, 0, _buffer.Length - toRead);
            _index = Math.Max(0, _index - toRead);
            Lock.ExitReadLock();
            return toRead;
        }
    }
}
