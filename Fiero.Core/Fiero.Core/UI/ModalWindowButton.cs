using System;

namespace Fiero.Core
{
    public readonly struct ModalWindowButton
    {
        public readonly string Label;
        public readonly bool? ResultType;

        public ModalWindowButton(string label, bool? result) { Label = label; ResultType = result; }

        public static readonly ModalWindowButton None = new(nameof(None), null);
        public static readonly ModalWindowButton ImplicitYes = new(nameof(ImplicitYes), true);
        public static readonly ModalWindowButton ImplicitNo = new(nameof(ImplicitNo), false);

        public static readonly ModalWindowButton Ok = new(nameof(Ok), true);
        public static readonly ModalWindowButton Cancel = new(nameof(Cancel), false);

        public static readonly ModalWindowButton Yes = new(nameof(Yes), true);
        public static readonly ModalWindowButton No = new(nameof(No), false);

        public static readonly ModalWindowButton Confirm = new(nameof(Confirm), true);
        public static readonly ModalWindowButton Close = new(nameof(Close), false);

        public override string ToString() => Label;
    }


}
