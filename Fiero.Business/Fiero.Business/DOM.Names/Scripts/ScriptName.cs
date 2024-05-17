namespace Fiero.Business
{
    public static class ScriptName
    {
        public const string Test = "test";
        // --- META ---
        // Handles the consequence of a dialogue node being triggered
        public const string Dialogue = "dialogue";
        // Decides which map script to use depending on the FloorId
        public const string Mapgen = "mapgen";
        // --- ITEMS ---
        // Implements bombs' ticking and exploding behavior
        public const string Bomb = "bomb";
        // Implements the grappling hook's effect
        public const string Grapple = "grapple";
        // Implements a reach effect for spear-like weapons
        public const string Reach = "reach";
        // -- ENVIRONMENT --
        // Implements barrels' explode-on-damage behavior
        public const string Barrel = "barrel";
        // -- MAPS --
        public const string MapTest = "map_test";

        public static IEnumerable<string> Preload()
        {
            yield return Test;
        }
    }
}
