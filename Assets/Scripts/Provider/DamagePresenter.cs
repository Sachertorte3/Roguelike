using System.Collections;
using System.Collections.Generic;
using Model.Game;
using UnityEngine;
using VContainer;
using View;
using Utilities;
using R3;

public class DamagePresenter
{
    [Inject]
    public DamagePresenter(World world, DamageTextSpawner damageTextSpawner)
    {
        world.ActiveMap.SubscribeToAllIgnoreNull(map =>
        {
            map.CharacterManager.CharacterEvents.OnDamageReceived.Subscribe(
                damageChanged =>
                {
                    damageTextSpawner.ShowDamage(damageChanged.Character.CurrentPosition, damageChanged.Message.Damage, (float)damageChanged.Message.Damage / damageChanged.Character.StatusManager.MaxHp);
                }
            );
            map.CharacterManager.CharacterEvents.OnHealReceived.Subscribe(
                healChanged =>
                {
                    damageTextSpawner.ShowHeal(healChanged.Character.CurrentPosition, healChanged.Message.Heal, (float)healChanged.Message.Heal / healChanged.Character.StatusManager.MaxHp);
                }
            );
        });
    }
}

