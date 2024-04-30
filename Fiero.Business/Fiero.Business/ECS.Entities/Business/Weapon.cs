using Ergo.Lang;

namespace Fiero.Business
{
    public class Weapon : Equipment
    {
        [RequiredComponent]
        [Term(Key = "props", Marshalling = TermMarshalling.Named)]
        public WeaponComponent WeaponProperties { get; private set; }
    }
}
