using System;
using System.Linq;

namespace Fiero.Core
{
    public abstract class UIControlResolverBase<T, TFonts, TTextures, TLocales, TSounds, TColors> : IUIControlResolver<T>
        where TTextures : struct, Enum
        where TLocales : struct, Enum
        where TSounds : struct, Enum
        where TColors : struct, Enum
        where TFonts : struct, Enum
        where T : UIControl
    {
        protected readonly GameUI UI;

        public UIControlResolverBase(
            GameUI ui
        ) {
            UI = ui;
        }

        public abstract T Resolve(LayoutGrid dom);
    }
}
