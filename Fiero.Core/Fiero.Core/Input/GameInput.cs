using SFML.System;
using SFML.Window;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Fiero.Core
{
    public class GameInput
    {
        private (long Timestamp, bool Delay, int KeyCode)? _kbRepeat;

        private readonly bool[] _kb1;
        private readonly bool[] _kb0;
        private readonly bool[] _m1;
        private readonly bool[] _m0;

        private readonly Stopwatch _sw;
        private Vector2i _mouse;

        public int KeyRepeatIntervalMs { get; set; } = 20;
        public int KeyRepeatDelayMs { get; set; } = 600;

        private volatile bool _focusStolen;
        private object _focusHolder;
        public bool IsKeyboardFocusAvailable => !_focusStolen;

        public GameInput()
        {
            _sw = new Stopwatch();
            _kb1 = new bool[(int)Keyboard.Key.KeyCount];
            _kb0 = new bool[(int)Keyboard.Key.KeyCount];
            _m1 = new bool[(int)Mouse.Button.ButtonCount];
            _m0 = new bool[(int)Mouse.Button.ButtonCount];
            _sw.Start();
        }

        public bool TryStealFocus(object owner)
        {
            if (_focusStolen)
                return false;
            _focusHolder = owner;
            return _focusStolen = true;
        }

        public bool TryRestoreFocus(object owner)
        {
            if (!_focusStolen || owner != _focusHolder)
                return false;
            _focusHolder = null;
            return !(_focusStolen = false);
        }

        public void Update(Vector2i mousePos)
        {
            _mouse = mousePos;
            for (var i = 0; i < (int)Keyboard.Key.KeyCount; i++)
            {
                var usedToBeUp = !_kb0[i];
                _kb0[i] = _kb1[i];
                _kb1[i] = Keyboard.IsKeyPressed((Keyboard.Key)i);
                if ((_kbRepeat == null || _kbRepeat.Value.KeyCode != i) && _kb0[i] && _kb1[i] && usedToBeUp)
                {
                    _kbRepeat = (_sw.ElapsedMilliseconds, true, i);
                }
                else if (_kbRepeat is { KeyCode: var keyCode, Delay: var delay, Timestamp: var millis })
                {
                    if (keyCode == i)
                    {
                        if (!_kb0[i] && !_kb1[i] && !usedToBeUp)
                        {
                            _kbRepeat = null;
                        }
                        else
                        {
                            var delta = _sw.ElapsedMilliseconds - millis;
                            if (delay && delta >= KeyRepeatDelayMs)
                            {
                                _kb0[i] = false;
                                _kbRepeat = (_sw.ElapsedMilliseconds, false, i);
                            }
                            else if (!delay && delta >= KeyRepeatIntervalMs)
                            {
                                _kb0[i] = false;
                                _kbRepeat = (_sw.ElapsedMilliseconds + KeyRepeatIntervalMs, false, i);
                            }
                        }
                    }
                }
            }
            for (var i = 0; i < (int)Mouse.Button.ButtonCount; i++)
            {
                _m0[i] = _m1[i];
                _m1[i] = Mouse.IsButtonPressed((Mouse.Button)i);
            }
        }

        public bool IsKeyDown(Keyboard.Key k) => _kb1[(int)k] && _kb0[(int)k];
        public bool IsKeyUp(Keyboard.Key k) => !_kb1[(int)k] && !_kb0[(int)k];
        public bool IsKeyPressed(Keyboard.Key k) => _kb1[(int)k] && !_kb0[(int)k];
        public bool IsKeyReleased(Keyboard.Key k) => !_kb1[(int)k] && _kb0[(int)k];

        public IEnumerable<Keyboard.Key> KeysPressed() => Enumerable.Range(0, (int)Keyboard.Key.KeyCount)
            .Where(i => _kb1[i] && !_kb0[i])
            .Select(i => (Keyboard.Key)i);
        public IEnumerable<Keyboard.Key> KeysReleased() => Enumerable.Range(0, (int)Keyboard.Key.KeyCount)
            .Where(i => !_kb1[i] && _kb0[i])
            .Select(i => (Keyboard.Key)i);
        public IEnumerable<Keyboard.Key> KeysDown() => Enumerable.Range(0, (int)Keyboard.Key.KeyCount)
            .Where(i => _kb1[i] && _kb0[i])
            .Select(i => (Keyboard.Key)i);
        public IEnumerable<Keyboard.Key> KeysUp() => Enumerable.Range(0, (int)Keyboard.Key.KeyCount)
            .Where(i => !_kb1[i] && !_kb0[i])
            .Select(i => (Keyboard.Key)i);

        public bool IsButtonDown(Mouse.Button k) => _m1[(int)k] && _m0[(int)k];
        public bool IsButtonUp(Mouse.Button k) => !_m1[(int)k] && !_m0[(int)k];
        public bool IsButtonPressed(Mouse.Button k) => _m1[(int)k] && !_m0[(int)k];
        public bool IsButtonReleased(Mouse.Button k) => !_m1[(int)k] && _m0[(int)k];

        public IEnumerable<Mouse.Button> ButtonsPressed() => Enumerable.Range(0, (int)Mouse.Button.ButtonCount)
            .Where(i => _m1[i] && !_m0[i])
            .Select(i => (Mouse.Button)i);
        public IEnumerable<Mouse.Button> ButtonsReleased() => Enumerable.Range(0, (int)Mouse.Button.ButtonCount)
            .Where(i => !_m1[i] && _m0[i])
            .Select(i => (Mouse.Button)i);
        public IEnumerable<Mouse.Button> ButtonsDown() => Enumerable.Range(0, (int)Mouse.Button.ButtonCount)
            .Where(i => _m1[i] && _m0[i])
            .Select(i => (Mouse.Button)i);
        public IEnumerable<Mouse.Button> ButtonsUp() => Enumerable.Range(0, (int)Mouse.Button.ButtonCount)
            .Where(i => !_m1[i] && !_m0[i])
            .Select(i => (Mouse.Button)i);

        public Coord GetMousePosition() => _mouse.ToCoord();
    }
}
