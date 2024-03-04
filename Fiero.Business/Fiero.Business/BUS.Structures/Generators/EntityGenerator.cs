namespace Fiero.Business
{
    public static class EntityGenerator
    {
        public static Func<GameEntityBuilders, IEntityBuilder<Weapon>> GenerateMeleeWeapon()
        {
            var candidates = new List<Func<GameEntityBuilders, IEntityBuilder<Weapon>>>
            {
                entities => entities.Weapon_Spear(),
                entities => entities.Weapon_Dagger(),
                entities => entities.Weapon_Hammer(),
                entities => entities.Weapon_Sword()
            };
            return Rng.Random.Choose(candidates);
        }
        public static Func<GameEntityBuilders, IEntityBuilder<Weapon>> GenerateRangedWeapon()
        {
            var candidates = new List<Func<GameEntityBuilders, IEntityBuilder<Weapon>>>
            {
                entities => entities.Weapon_Bow(),
                entities => entities.Weapon_Crossbow()
            };
            return Rng.Random.Choose(candidates);
        }
        public static Func<GameEntityBuilders, IEntityBuilder<Weapon>> GenerateWeapon()
        {
            return Rng.Random.Choose([GenerateMeleeWeapon(), GenerateRangedWeapon()]);
        }

        public static IEntityBuilder<T> Enchant<T>(IEntityBuilder<T> entity, int magnitude = 1)
            where T : DrawableEntity
        {
            return entity
                .WithIntrinsicEffect(EffectGenerator.GenerateDef(magnitude), EffectGenerator.GenerateModifier<T>())
                .WithColor(Rng.Random.Choose(Enum.GetValues<ColorName>().Except([ColorName.Black, ColorName.White]).ToArray()));
        }
    }
}
