using Fiero.Core;

namespace Fiero.Business
{
    public static partial class Data
    {
        public static class Global
        {
            public static readonly GameDatum<int> RngSeed = new(nameof(Global) + nameof(RngSeed));

        }

    }
}
