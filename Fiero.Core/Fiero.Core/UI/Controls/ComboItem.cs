using SFML.Graphics;
using System;

namespace Fiero.Core
{
    public class ComboItem : Label
    {
        public ComboItem(GameInput input, Frame frame, int maxLength, Func<string, Text> getText) : base(input, maxLength, getText)
        {
            Clickable = true;
            Children.Add(frame);
        }
    }
}
