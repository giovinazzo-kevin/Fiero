using Unconcern.Common;

namespace Fiero.Business
{
    public partial class MusicSystem : EcsSystem
    {
        public readonly Tracker Tracker;

        public MusicSystem(EventBus bus) : base(bus)
        {
            Tracker = new(16);
            Tracker.Tempo.V = 140;
            Tracker.RowsPerPattern.V = 32;
            Tracker.Instruments.Add(
                new Instrument(() => new Oscillator(OscillatorShape.Saw),
                               () => new Envelope(delay: 0.01f)));
            Tracker.Instruments.Add(
                new Instrument(() => new Oscillator(OscillatorShape.Square),
                               () => new Envelope(decay: 0.1f, sustain: 0.8f)));
            Tracker.Instruments.Add(
                new Instrument(() => new Oscillator(OscillatorShape.Triangle),
                               () => new Envelope(decay: 0.1f, sustain: 0.9f)));
            int R = 0;
            Tracker.Mixer.Tracks[0].Attach(Tracker.Instruments[0]);
            Tracker.Mixer.Tracks[1].Attach(Tracker.Instruments[1]);
            Tracker.Mixer.Tracks[2].Attach(Tracker.Instruments[2]);
            Tracker.Play();
            void SetRow(int channel, int rowNum, TrackerChannelRow row)
            {
                Tracker.Channels[channel].SetRow(rowNum, row);
            }
        }


        public void Update(TimeSpan t, TimeSpan dt)
        {
            Tracker.Update(dt, out int row);
        }
    }
}
