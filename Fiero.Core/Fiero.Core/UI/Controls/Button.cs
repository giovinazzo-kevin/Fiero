using SFML.Graphics;
using System;

namespace Fiero.Core
{
    public class Button : Label
    {
        public Button(GameInput input, Func<string, int, Text> getText) : base(input, getText)
        {
            IsInteractive.V = true;
        }
    }
}
