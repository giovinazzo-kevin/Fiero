namespace Fiero.Core
{

    public class Envelope : ISynthesizer
    {
        public readonly Knob<float> Delay = new(0, 1, 0.0f);
        public readonly Knob<float> Attack = new(0, 1, 0.02f);
        public readonly Knob<float> Hold = new(0, 1, 0.1f);
        public readonly Knob<float> Decay = new(0, 1, 0.8f);
        public readonly Knob<float> Sustain = new(0, 1, 0.5f);
        public readonly Knob<float> Release = new(0, 1, 0.4f);

        public readonly Knob<bool> Invert = new(false, true, false);

        const double _minimumLevel = 0.0001;
        private double _level, _multiplier;
        private uint _currentSample, _nextStateSample, _sampleRate = 44100;
        private readonly int _stateCount = Enum.GetValues<EnvelopeState>().Length;

        public EnvelopeState State { get; private set; }

        public Envelope(
            float delay = 0f, float attack = 0.02f, float hold = 0.1f, float decay = 0.8f, float sustain = 0.5f, float release = 0.4f,
            bool invert = false)
        {
            Delay.V = delay;
            Attack.V = attack;
            Hold.V = hold;
            Decay.V = decay;
            Sustain.V = sustain;
            Release.V = release;
            Invert.V = invert;
        }

        public void Engage()
        {
            EnterState(EnvelopeState.Delay);
        }

        public void Disengage()
        {
            EnterState(EnvelopeState.Release);
        }

        private double GetMultiplier(double startLevel, double endLevel, uint lengthInSamples)
        {
            return 1.0 + (Math.Log(endLevel) - Math.Log(startLevel)) / lengthInSamples;
        }

        private void EnterState(EnvelopeState newState)
        {
            State = newState;
            _currentSample = 0;
            switch (State)
            {
                case EnvelopeState.Delay:
                    _nextStateSample = (uint)(_sampleRate * Delay);
                    _level = 0;
                    _multiplier = 1;
                    break;
                case EnvelopeState.Attack:
                    _nextStateSample = (uint)(_sampleRate * Attack);
                    _level = _minimumLevel;
                    _multiplier = GetMultiplier(_level, 1, _nextStateSample);
                    break;
                case EnvelopeState.Hold:
                    _nextStateSample = (uint)(_sampleRate * Hold);
                    _level = 1;
                    _multiplier = 1;
                    break;
                case EnvelopeState.Decay:
                    _nextStateSample = (uint)(_sampleRate * Decay);
                    _level = 1;
                    _multiplier = GetMultiplier(_level, Math.Max(Sustain.V, _minimumLevel), _nextStateSample);
                    break;
                case EnvelopeState.Sustain:
                    _nextStateSample = 0;
                    _level = Sustain.V;
                    _multiplier = 1;
                    break;
                case EnvelopeState.Release:
                    _nextStateSample = (uint)(_sampleRate * Release);
                    _multiplier = GetMultiplier(_level, _minimumLevel, _nextStateSample);
                    break;
                default:
                case EnvelopeState.Off:
                    _nextStateSample = 0;
                    _level = 0;
                    _multiplier = 1;
                    break;
            }
        }

        public bool NextSample(int sr, float t, out double sample)
        {
            _sampleRate = (uint)sr;

            if (State != EnvelopeState.Off && State != EnvelopeState.Sustain)
            {
                if (_currentSample == _nextStateSample)
                {
                    EnterState((EnvelopeState)((int)(State + 1) % _stateCount));
                }
                _level *= _multiplier;
                _currentSample++;
            }

            sample = _level;
            return true;
        }
    }
}
