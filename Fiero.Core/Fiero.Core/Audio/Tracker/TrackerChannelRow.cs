namespace Fiero.Core
{
    public readonly struct TrackerChannelRow
    {
        public static TrackerChannelRow Empty() => new(TrackerNote.__, 4, 1, 255);

        public readonly TrackerNote Note;
        public readonly byte Octave;
        public readonly byte Instrument;
        public readonly byte Volume;

        public TrackerChannelRow(TrackerNote note, byte octave, byte instrument, byte volume)
        {
            Note = note;
            Octave = octave;
            Instrument = instrument;
            Volume = volume;
        }

        public TrackerChannelRow WithNote(TrackerNote note) => new(note, Octave, Instrument, Volume);
        public TrackerChannelRow WithOctave(byte octave) => new(Note, octave, Instrument, Volume);
        public TrackerChannelRow WithInstrument(byte instrument) => new(Note, Octave, instrument, Volume);
        public TrackerChannelRow WithVolume(byte volume) => new(Note, Octave, Instrument, volume);
    }
}
