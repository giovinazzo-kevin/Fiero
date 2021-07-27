using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct FactionGlossary
    {
        public readonly FactionNames Names;


        public FactionGlossary(FactionNames names)
        {
            Names = names;
        }
    }
}
