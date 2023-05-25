using System;
using System.Runtime.InteropServices;
using System.Text;

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

        public static string GetCharsFromKeys(VirtualKeys keys, byte[] lpKeyState)
        {
            byte[] keyboardState = lpKeyState;
            StringBuilder pwszBuff = new StringBuilder();
            int ascii = ToUnicodeEx((uint)keys, 0, keyboardState, pwszBuff, 2, 0, GetKeyboardLayout(0));
            return pwszBuff.ToString();
        }
    }
}
