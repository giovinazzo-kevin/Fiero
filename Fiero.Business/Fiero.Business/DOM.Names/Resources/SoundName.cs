using System.Reflection;

namespace Fiero.Business
{
    public static class SoundName
    {
        public const string Blip = nameof(Blip);
        public const string Ok = nameof(Ok);
        public const string WallBump = nameof(WallBump);
        public const string SpellCast = nameof(SpellCast);
        public const string ItemUsed = nameof(ItemUsed);
        public const string Buff = nameof(Buff);
        public const string Debuff = nameof(Debuff);
        public const string ItemPickedUp = nameof(ItemPickedUp);
        public const string BossSpotted = nameof(BossSpotted);
        public const string TrapSpotted = nameof(TrapSpotted);
        public const string MeleeAttack = nameof(MeleeAttack);
        public const string RangedAttack = nameof(RangedAttack);
        public const string MagicAttack = nameof(MagicAttack);
        public const string EnemyDeath = nameof(EnemyDeath);
        public const string PlayerDeath = nameof(PlayerDeath);
        public const string Explosion = nameof(Explosion);
        public const string Crit = nameof(Crit);
        public const string Countdown3 = nameof(Countdown3);
        public const string Countdown2 = nameof(Countdown2);
        public const string Countdown1 = nameof(Countdown1);
        public const string MonsterLevelUp = nameof(MonsterLevelUp);
        public const string PlayerLevelUp = nameof(PlayerLevelUp);
        public const string Splash = nameof(Splash);
        public const string SplashLarge = nameof(SplashLarge);

        public static readonly string[] _Values = typeof(SoundName).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.Name != nameof(_Values))
            .Select(x => (string)x.GetValue(null))
            .ToArray();
    }
}
