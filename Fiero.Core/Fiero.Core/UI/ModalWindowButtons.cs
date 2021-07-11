using System;

namespace Fiero.Core
{
    [Flags]
    public enum ModalWindowButtons : int
    {
        None = 0,
        Ok = 1,
        Cancel = 2,
        Yes = 4,
        No = 8,
        Close = 16,
        ImplicitYes = 32,
        ImplicitNo = 64
    }
}
