namespace Fiero.Business
{
    /// <summary>
    /// Scripts defined here will be loaded and routed automatically at game startup time.
    /// Scripts NOT defined here will have to be loaded and routed manually.
    /// </summary>
    public enum ScriptName
    {
        Test,

        // --- META ---
        // Handles serialization of entities
        Entity,
        // Handles the consequence of a dialogue node being triggered
        Dialogue,
        // Decides which map script to use depending on the FloorId
        Mapgen,
        // --- ITEMS ---
        // Implements bombs' ticking and exploding behavior
        Bomb,
        // Implements the grappling hook's effect
        Grapple,
        // Implements a reach effect for spear-like weapons
        Reach,
        // -- ENVIRONMENT --
        // Implements barrels' explode-on-damage behavior
        Barrel,
        // -- MAPS --
        MapTest
    }
}
