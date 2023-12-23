using SFML.System;
using SFML.Window;
using System.Diagnostics;

namespace Fiero.Core
{

    public class GameInput
    {
        private (long Timestamp, bool Delay, int KeyCode)? _kbRepeat;
        private readonly bool[] _kb0, _kb1;
        private readonly bool[] _m0, _m1;
        private float _sw0, _sw1;

        private readonly Stopwatch _stopWatch;
        private Vector2i _mouse;

        private volatile bool _focusStolen;
        private object _focusHolder;
        public bool IsKeyboardFocusAvailable => !_focusStolen;
        public int KeyRepeatIntervalMs { get; set; } = 0;
        public int KeyRepeatDelayMs { get; set; } = 600;
        public readonly byte[] KeyboardState;

        protected readonly GameWindow Window;

        public GameInput(GameWindow win)
        {
            Window = win;
            _stopWatch = new Stopwatch();
            _kb0 = new bool[256];
            _kb1 = new bool[_kb0.Length];
            KeyboardState = new byte[_kb0.Length];
            _m0 = new bool[(int)Mouse.Button.ButtonCount];
            _m1 = new bool[_m0.Length];
            win.RenderWindowChanged += (_, old) =>
            {
                if (old != null)
                {
                    old.MouseButtonPressed -= RenderWindow_MouseButtonPressed;
                    old.MouseButtonReleased -= RenderWindow_MouseButtonReleased;
                    old.MouseWheelScrolled -= RenderWindow_MouseWheelScrolled;
                }
                win.RenderWindow.MouseButtonPressed += RenderWindow_MouseButtonPressed;
                win.RenderWindow.MouseButtonReleased += RenderWindow_MouseButtonReleased;
                win.RenderWindow.MouseWheelScrolled += RenderWindow_MouseWheelScrolled;
            };
            _stopWatch.Start();
        }

        private void RenderWindow_MouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            _m1[(int)e.Button] = false;
        }
        private void RenderWindow_MouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            _m1[(int)e.Button] = true;
        }
        private void RenderWindow_MouseWheelScrolled(object sender, MouseWheelScrollEventArgs e)
        {
            _sw1 = e.Delta;
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

        public void Clear()
        {
            Array.Clear(_kb1);
            Array.Clear(_kb0);
            Array.Clear(_m1);
            Array.Clear(_m0);
            _sw0 = _sw1 = 0;
        }

        public void Update()
        {
            _mouse = Window.GetMousePosition();
            Array.Copy(_m1, _m0, _m0.Length);
            for (var i = 0; i < _kb0.Length; i++)
            {
                _kb0[i] = _kb1[i];
                _kb1[i] = (WinKeyboardState.GetAsyncKeyState(i) & 0x8000) != 0;
                KeyboardState[i] = (byte)(_kb1[i] ? 0xFF : 0);
                if ((_kbRepeat == null || _kbRepeat.Value.KeyCode != i) && IsKeyPressed((VirtualKeys)i))
                {
                    _kbRepeat = (_stopWatch.ElapsedMilliseconds, true, i);
                }
                else if (_kbRepeat is { KeyCode: var keyCode, Delay: var delay, Timestamp: var millis })
                {
                    if (keyCode == i)
                    {
                        if (IsKeyReleased((VirtualKeys)i))
                        {
                            _kbRepeat = null;
                        }
                        else
                        {
                            var delta = _stopWatch.ElapsedMilliseconds - millis;
                            if (delay && delta >= KeyRepeatDelayMs)
                            {
                                _kb0[i] = false;
                                _kbRepeat = (_stopWatch.ElapsedMilliseconds, false, i);
                            }
                            else if (!delay && delta >= KeyRepeatIntervalMs)
                            {
                                _kb0[i] = false;
                                _kbRepeat = (_stopWatch.ElapsedMilliseconds + KeyRepeatIntervalMs, false, i);
                            }
                        }
                    }
                }
            }
            _sw0 = _sw1;
            _sw1 = 0;
        }

        public bool IsKeyDown(VirtualKeys k) => _kb1[(int)k] && _kb0[(int)k];
        public bool IsKeyUp(VirtualKeys k) => !_kb1[(int)k] && !_kb0[(int)k];
        public bool IsKeyPressed(VirtualKeys k) => _kb1[(int)k] && !_kb0[(int)k];
        public bool IsKeyReleased(VirtualKeys k) => !_kb1[(int)k] && _kb0[(int)k];

        public bool IsMouseWheelScrollingDown() => _sw1 != 0 && _sw0 > _sw1;
        public bool IsMouseWheelScrollingUp() => _sw1 != 0 && _sw0 < _sw1;

        public IEnumerable<VirtualKeys> KeysPressed => Enum.GetValues<VirtualKeys>()
            .Where(IsKeyPressed);
        public IEnumerable<VirtualKeys> KeysReleased => Enum.GetValues<VirtualKeys>()
            .Where(IsKeyReleased);
        public IEnumerable<VirtualKeys> KeysDown => Enum.GetValues<VirtualKeys>()
            .Where(IsKeyDown);
        public IEnumerable<VirtualKeys> KeysUp => Enum.GetValues<VirtualKeys>()
            .Where(IsKeyUp);

        public bool IsButtonDown(Mouse.Button k) => _m1[(int)k] && _m0[(int)k];
        public bool IsButtonUp(Mouse.Button k) => !_m1[(int)k] && !_m0[(int)k];
        public bool IsButtonPressed(Mouse.Button k) => _m1[(int)k] && !_m0[(int)k];
        public bool IsButtonReleased(Mouse.Button k) => !_m1[(int)k] && _m0[(int)k];

        public IEnumerable<Mouse.Button> ButtonsPressed => Enum.GetValues<Mouse.Button>()
            .Where(b => b >= 0 && b < Mouse.Button.ButtonCount)
            .Where(IsButtonPressed);
        public IEnumerable<Mouse.Button> ButtonsReleased => Enum.GetValues<Mouse.Button>()
            .Where(b => b >= 0 && b < Mouse.Button.ButtonCount)
            .Where(IsButtonReleased);
        public IEnumerable<Mouse.Button> ButtonsDown => Enum.GetValues<Mouse.Button>()
            .Where(b => b >= 0 && b < Mouse.Button.ButtonCount)
            .Where(IsButtonDown);
        public IEnumerable<Mouse.Button> ButtonsUp => Enum.GetValues<Mouse.Button>()
            .Where(b => b >= 0 && b < Mouse.Button.ButtonCount)
            .Where(IsButtonUp);

        public Coord GetMousePosition() => _mouse.ToCoord();
    }
}
