using System.Runtime.InteropServices;

namespace Fiero.Core
{
    public class WinKeyboardState
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int wVirtKey);

        [DllImport("user32.dll")]
        public static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        public static extern IntPtr GetKeyboardLayout(uint idThread);

        public static string GetCharsFromKeys(VirtualKeys keys, byte[] lpKeyState, bool? shift = null, bool? ctrl = null, bool? alt = null)
        {
            byte[] keyboardState = lpKeyState;
            StringBuilder pwszBuff = new StringBuilder();

            var (oldShift, oldCtrl, oldAlt) = (lpKeyState[(int)VirtualKeys.Shift], lpKeyState[(int)VirtualKeys.Control], lpKeyState[(int)VirtualKeys.Menu]);
            if (shift.HasValue)
                lpKeyState[(int)VirtualKeys.Shift] = (byte)(shift.Value ? 0x80 : 0x00);
            if (ctrl.HasValue)
                lpKeyState[(int)VirtualKeys.Control] = (byte)(ctrl.Value ? 0x80 : 0x00);
            if (alt.HasValue)
                lpKeyState[(int)VirtualKeys.Menu] = (byte)(alt.Value ? 0x80 : 0x00);
            int ascii = ToUnicodeEx((uint)keys, 0, keyboardState, pwszBuff, 2, 0, GetKeyboardLayout(0));
            (lpKeyState[(int)VirtualKeys.Shift], lpKeyState[(int)VirtualKeys.Control], lpKeyState[(int)VirtualKeys.Menu]) = (oldShift, oldCtrl, oldAlt);
            return pwszBuff.ToString();
        }
    }
}
