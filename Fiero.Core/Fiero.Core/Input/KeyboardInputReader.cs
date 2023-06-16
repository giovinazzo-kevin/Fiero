namespace Fiero.Core
{
    [SingletonDependency]
    public sealed class KeyboardInputReader
    {
        public readonly GameInput Input;
        public readonly GameLoop Loop;
        private readonly Queue<char> _queue = new();

        public KeyboardInputReader(GameInput input, GameLoop loop)
        {
            Input = input;
            Loop = loop;
            Loop.Update += Loop_Update;
        }

        private void Loop_Update(float arg1, float arg2)
        {
            _queue.Clear();
            foreach (var ch in Input.KeysPressed
                .SelectMany(key => WinKeyboardState.GetCharsFromKeys(key, Input.KeyboardState)))
            {
                _queue.Enqueue(ch);
            }
        }

        public char ReadCharBlocking(bool consume = false)
        {
            var ret = '\0';
            while (ret == '\0')
            {
                if (TryReadCharInternal(out ret, consume))
                    break;
            }
            return ret;
        }

        private bool TryReadCharInternal(out char ret, bool consume)
        {
            if (consume)
            {
                if (_queue.TryDequeue(out ret))
                    return true;
            }
            else
            {
                if (_queue.TryPeek(out ret))
                    return true;
            }
            return false;
        }

        public bool TryReadChar(out char ret, bool consume = false)
        {
            return TryReadCharInternal(out ret, consume);
        }
    }
}
