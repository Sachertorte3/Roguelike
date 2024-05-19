using System;

namespace Utilities
{
    public enum ParticleType
    {
        Blind,
        Blood,
        BloodRage,
        Burn,
        Cold,
        Confusion,
        Electric,
        Energy,
        FastHeal,
        FastSpeed,
        Fear,
        HealGreen,
        Paralysis,
        PoisoningSmoke,
        PoisoningBubble,
        PowerUp,
        Relieve,
        ShineyStar,
        Sleep,
        SlowDown,
        Stuned,
        SuckBlood,
        Rage
    }
    public static class EffectPrefabTypeExtension
    {
        public static string GetPath(this ParticleType particleType)
        {
            return $"EffectPrefabs/{particleType.GetFileName()}.prefab";
        }

        public static string GetFileName(this ParticleType particleType)
        {
            return particleType switch
            {
                ParticleType.Blind => "effect_state_blind",
                ParticleType.Blood => "effect_state_blood",
                ParticleType.BloodRage => "effect_state_bloodRage",
                ParticleType.Burn => "effect_state_burn",
                ParticleType.Cold => "effect_state_coldSnow",
                ParticleType.Confusion => "effect_state_confusion",
                ParticleType.Electric => "effect_state_electric",
                ParticleType.Energy => "effect_state_energy",
                ParticleType.FastHeal => "effect_state_fastHeal",
                ParticleType.FastSpeed => "effect_state_fastSpeed",
                ParticleType.Fear => "effect_state_fear",
                ParticleType.HealGreen => "effect_state_healGreen",
                ParticleType.Paralysis => "effect_state_paralysis",
                ParticleType.PoisoningSmoke => "effect_state_poisoning_1",
                ParticleType.PoisoningBubble => "effect_state_poisoning_2",
                ParticleType.PowerUp => "effect_state_powerUp",
                ParticleType.Relieve => "effect_state_relieve",
                ParticleType.ShineyStar => "effect_state_shineyStar",
                ParticleType.Sleep => "effect_state_sleep",
                ParticleType.SlowDown => "effect_state_slowDown",
                ParticleType.Stuned => "effect_state_stuned",
                ParticleType.SuckBlood => "effect_state_suckBlood",
                ParticleType.Rage => "effect_statel_rage",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
