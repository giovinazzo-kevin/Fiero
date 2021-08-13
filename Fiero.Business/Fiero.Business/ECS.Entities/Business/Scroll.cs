using Fiero.Core;

namespace Fiero.Business
{
    public class Scroll : Throwable
    {
        [RequiredComponent]
        public ScrollComponent ScrollProperties { get; private set; }
    }
}
