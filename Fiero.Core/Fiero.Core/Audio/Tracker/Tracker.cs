namespace Fiero.Core
{
    public class Tracker
    {
        private TimeSpan _accumulator, _interval;
        private readonly TrackerChannel[] _channels;

        public readonly Mixer Mixer;

        public IReadOnlyList<TrackerChannel> Channels => _channels;
        public readonly List<Instrument> Instruments = new();

        public TimeSpan Time { get; private set; }
        public int Row { get; private set; }

        public event Action<Tracker, TrackerState> StateChanged;
        public TrackerState State { get; private set; }

        public Knob<int> Tempo { get; private set; }
        public Knob<int> RowsPerPattern { get; private set; }

        public event Action<Tracker, int> Step;

        public Tracker(int nChannels, Mixer mixer = null)
        {
            _channels = new TrackerChannel[nChannels];
            for (int i = 0; i < nChannels; i++)
            {
                _channels[i] = new();
            }
            Tempo = new(min: 1, max: 1000, init: 100, OnTempoChanged);
            RowsPerPattern = new(min: 1, max: 64, init: 16, OnRowsPerPatternChanged);
            Mixer = mixer ?? new Mixer(nChannels, 44100);
        }

        protected virtual void OnTempoChanged(int tempo)
        {
            // tempo is BPM, but we want BPS and then we divide that by 4 to get quarter note rhythms
            _interval = TimeSpan.FromSeconds(60f / tempo) / 4;
        }

        protected virtual void OnRowsPerPatternChanged(int rowsPerPattern)
        {
            foreach (var chan in Channels)
            {
                chan.Resize(rowsPerPattern);
            }
        }

        public void Play()
        {
            State = TrackerState.Playing;
            StateChanged?.Invoke(this, State);
            Mixer.Play();
        }

        public void Stop()
        {
            Row = 0;
            State = TrackerState.Stopped;
            StateChanged?.Invoke(this, State);
            Mixer.Stop();
        }

        public void Pause()
        {
            State = TrackerState.Paused;
            StateChanged?.Invoke(this, State);
        }

        protected bool UpdateChannels(TimeSpan delta, out TrackerChannelRow[] rowsToPlay)
        {
            rowsToPlay = Array.Empty<TrackerChannelRow>();
            if (State != TrackerState.Playing)
                return false;

            if ((_accumulator += delta) >= _interval)
            {
                _accumulator = TimeSpan.Zero;
                var row = Row;
                rowsToPlay = Channels.Select(c => c.GetRow(row))
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
            switch (row.Note)
            {
                case TrackerNote.__:
                    break;
                case TrackerNote._S:
                    instrument.StopAll();
                    break;
                default:
                    instrument.Play((Note)(int)row.Note, row.Octave, _interval, row.Volume / 255f);
                    break;
            }
        }

        public bool Update(TimeSpan delta, out int playedRow)
        {
            playedRow = Row;
            if (UpdateChannels(delta, out var rowsToPlay))
            {
                foreach (var row in rowsToPlay)
                {
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
