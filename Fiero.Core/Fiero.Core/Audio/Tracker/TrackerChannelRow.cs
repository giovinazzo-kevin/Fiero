namespace Fiero.Core
{
    public readonly struct TrackerChannelRow
    {
        public static TrackerChannelRow Empty(byte id) => new(id, TrackerNote.None, 4, 0, 255);

        public readonly byte Id;
        public readonly TrackerNote Note;
        public readonly byte Octave;
        public readonly byte Instrument;
        public readonly byte Volume;

        public TrackerChannelRow(byte id, TrackerNote note, byte octave, byte instrument, byte volume)
        {
            Id = id;
            Note = note;
            Octave = octave;
            Instrument = instrument;
            Volume = volume;
        }

    }
}
