using System;

namespace Fiero.Business
{
    [Flags]
    public enum VisibilityName
    {
        // Can't see anything / can't be seen by anything
        Blind = 0,
        // Can see things that are visible
        Visible = 1,
        // Can see things that are magically invisible
        Invisible = 2,
        // Can see things that are hidden (like traps and fake walls)
        Hidden = 4,
        // Can see things that are obscured by darkness (night vision)
        Obscured = 8,
        // Can see everything
        TrueSight = Visible | Invisible | Hidden | Obscured
    }
}
