using LightInject;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Fiero.Core
{


    public class Tracker
    {
        private float _accumulator, _interval;
        private readonly TrackerChannel[] _channels;

        public readonly Mixer Mixer;

        public IReadOnlyList<TrackerChannel> Channels => _channels;
        public readonly List<Instrument> Instruments = new();

        public float Time { get; private set; }
        public int Row { get; private set; }
        public TrackerState State { get; private set; }

        public Knob<int> Tempo { get; private set; }

        public Tracker(int nChannels, Mixer mixer = null)
        {
            _channels = new TrackerChannel[nChannels];
            for (int i = 0; i < nChannels; i++) {
                _channels[i] = new();
            }
            Tempo = new(min: 1, max: 500, init: 100, OnTempoChanged);
            Mixer = mixer ?? new Mixer(nChannels, 44100);
        }

        protected virtual void OnTempoChanged(int tempo)
        {
            _interval = 1f / tempo;
        }

        public void Start()
        {
            State = TrackerState.Playing;
        }

        public void Stop()
        {
            Row = 0;
            State = TrackerState.Stopped;
        }

        public void Pause()
        {
            State = TrackerState.Paused;
        }

        protected bool UpdateChannels(float delta, out TrackerChannelRow[] rowsToPlay)
        {
            rowsToPlay = Array.Empty<TrackerChannelRow>();
            if (State != TrackerState.Playing)
                return false;

            if((_accumulator += delta) >= _interval) {
                _accumulator = 0;
                rowsToPlay = Channels.Select(c => c.GetRow(++Row))
                    .ToArray();
                return true;
            }

            Time += delta;
            return false;
        }

        public void Update(float delta)
        {
            if(UpdateChannels(delta, out var rowsToPlay)) {
                foreach (var row in rowsToPlay) {
                    if (row.Instrument == 0 || row.Volume == 0)
                        continue;
                    if (row.Instrument >= Instruments.Count)
                        continue;

                    var instrument = Instruments[row.Instrument];
                }
            }
        }
    }
}
