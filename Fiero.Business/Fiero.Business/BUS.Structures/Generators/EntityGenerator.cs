namespace Fiero.Business
{
    public static class EntityGenerator
    {
        public static IEntityBuilder<Weapon> GenerateMeleeWeapon(GameEntityBuilders builders)
        {
            var candidates = new List<Func<GameEntityBuilders, IEntityBuilder<Weapon>>>
            {
                entities => entities.Weapon_Spear(),
                entities => entities.Weapon_Dagger(),
                entities => entities.Weapon_Hammer(),
                entities => entities.Weapon_Sword()
            };
            return Rng.Random.Choose(candidates)(builders);
        }
        public static IEntityBuilder<Weapon> GenerateRangedWeapon(GameEntityBuilders builders)
        {
            var candidates = new List<Func<GameEntityBuilders, IEntityBuilder<Weapon>>>
            {
                entities => entities.Weapon_Bow(),
                entities => entities.Weapon_Crossbow()
            };
            return Rng.Random.Choose(candidates)(builders);
        }
        public static IEntityBuilder<Weapon> GenerateWeapon(GameEntityBuilders builders)
        {
            return Rng.Random.Choose([GenerateMeleeWeapon, GenerateRangedWeapon])(builders);
        }

        public static IEntityBuilder<T> Enchant<T>(IEntityBuilder<T> entity, int magnitude = 1)
            where T : DrawableEntity
        {
            return entity
                .WithIntrinsicEffect(EffectGenerator.GenerateDef(magnitude), EffectGenerator.GenerateModifier<T>())
                .WithColor(Rng.Random.Choose(ColorName._Values.Except([ColorName.Black, ColorName.White]).ToArray()));
        }
    }
}
