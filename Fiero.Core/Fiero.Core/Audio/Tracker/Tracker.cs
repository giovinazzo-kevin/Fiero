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

        public event Action<Tracker, TrackerState> StateChanged;
        public TrackerState State { get; private set; }

        public Knob<int> Tempo { get; private set; }
        public Knob<int> RowsPerPattern { get; private set; }

        public event Action<Tracker, int> Step;

        public Tracker(int nChannels, Mixer mixer = null)
        {
            _channels = new TrackerChannel[nChannels];
            for (int i = 0; i < nChannels; i++) {
                _channels[i] = new();
            }
            Tempo = new(min: 1, max: 1000, init: 100, OnTempoChanged);
            RowsPerPattern = new(min: 1, max: 64, init: 16, OnRowsPerPatternChanged);
            Mixer = mixer ?? new Mixer(nChannels, 44100);
        }

        protected virtual void OnTempoChanged(int tempo)
        {
            // tempo is BPM, but we want BPS
            _interval = 1f / (tempo / 60f);
        }

        protected virtual void OnRowsPerPatternChanged(int rowsPerPattern)
        {
            foreach (var chan in Channels) {
                chan.Resize(rowsPerPattern);
            }
        }

        public void Play()
        {
            State = TrackerState.Playing;
            StateChanged?.Invoke(this, State);
        }

        public void Stop()
        {
            Row = 0;
            State = TrackerState.Stopped;
            StateChanged?.Invoke(this, State);
        }

        public void Pause()
        {
            State = TrackerState.Paused;
            StateChanged?.Invoke(this, State);
        }

        protected bool UpdateChannels(float delta, out TrackerChannelRow[] rowsToPlay)
        {
            rowsToPlay = Array.Empty<TrackerChannelRow>();
            if (State != TrackerState.Playing)
                return false;

            if((_accumulator += delta) >= _interval) {
                _accumulator = 0;
                rowsToPlay = Channels.Select(c => c.GetRow(Row))
                    .ToArray();
                return true;
            }

            Time += delta;
            return false;
        }

        public void PlayRow(TrackerChannelRow row)
        {
            if (row.Instrument == 0 || row.Volume == 0)
                return;
            if (row.Instrument - 1 >= Instruments.Count)
                return;
            var instrument = Instruments[row.Instrument - 1];
            switch (row.Note) {
                case TrackerNote.None:
                    break;
                case TrackerNote.Stop:
                    instrument.StopAll();
                    break;
                default:
                    instrument.Play((Note)(int)row.Note, row.Octave, _interval, row.Volume / 255f);
                    break;
            }
        }

        public bool Update(float delta, out int playedRow)
        {
            playedRow = Row;
            if(UpdateChannels(delta, out var rowsToPlay)) {
                foreach (var row in rowsToPlay) {
                    PlayRow(row);
                }
                Step?.Invoke(this, Row);
                Row = (Row + 1) % RowsPerPattern;
                return true;
            }
            return false;
        }
    }
}
