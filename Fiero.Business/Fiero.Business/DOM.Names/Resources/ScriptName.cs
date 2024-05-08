namespace Fiero.Business
{
    public enum ScriptName
    {
        Test,
        // Handles serialization of entities
        Entity,
        // Handles the consequence of a dialogue node being triggered
        Dialogue,
        // Implements bombs' ticking and exploding behavior
        Bomb,
        // Implements the grappling hook's effect
        Grapple,
        // Implements a reach effect for spear-like weapons
        Reach,
        // MAPS
        Map_Test,
    }
}
