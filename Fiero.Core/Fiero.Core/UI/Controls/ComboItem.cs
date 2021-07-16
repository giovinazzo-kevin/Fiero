using SFML.Graphics;
using System;

namespace Fiero.Core
{
    public class ComboItem : Label
    {
        public ComboItem(GameInput input, Func<string, BitmapText> getText) : base(input, getText)
        {
            IsInteractive.V = true;
        }
    }
}
