using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Service.Effect;
using UnityEngine;

namespace Domain.Service.Items
{
    public static class WeaponFactory
    {
        public static ItemMemento Create(ItemData weapon, WeaponPrefix prefix)
        {
            var item = Item.Build(weapon);
            var effects = item.SkillOnUse.Value;
            if (effects != null && effects is SpawnEffectSkillMemento spawnEffectSkillMemento)
            {
                foreach (var effect in spawnEffectSkillMemento.Effect)
                {
                    if (effect is AttackEffect attackEffect)
                    {
                        attackEffect.MultiplyPower(prefix.PowerMagnification);
                    }
                    else if (effect is AbsorbsEffect absorbsEffect)
                    {
                        absorbsEffect.MultiplyPower(prefix.PowerMagnification);
                    }
                }

                foreach (var effect in prefix.AdditionalEffects)
                {
                    spawnEffectSkillMemento.Effect.Add(effect);
                }
            }
            item = item.CopyWith(
                name: prefix.Name + item.Name,
                maxUsages: Mathf.RoundToInt(item.MaxUsages * prefix.UsageLimitMagnification),
                remainingUsages: Mathf.RoundToInt(item.RemainingUsages * prefix.UsageLimitMagnification),
                upgradeLimit: item.UpgradeLimit + prefix.AdditionalUpgradeLimit
            );
            return item;
        }
    }
}