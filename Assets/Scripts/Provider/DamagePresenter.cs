using System.Collections;
using System.Collections.Generic;
using Model.Game;
using UnityEngine;
using VContainer;
using View;
using Utilities;
using R3;
using Data.Setting;

public class DamagePresenter
{
    [Inject]
    public DamagePresenter(World world, DamageTextSpawner damageTextSpawner, FlushController flushController)
    {
        world.ActiveMap.SubscribeToAllIgnoreNull(map =>
        {
            map.CharacterManager.PlayerEvents.OnDamageReceived.Subscribe(
                damageChanged =>
                {
                    if (damageChanged.Character.Visibility.CurrentValue == true)
                    {
                        var damagePercentageFromMaxHp = (float)damageChanged.Message.Damage / damageChanged.Character.StatusManager.MaxHp;
                        var hpPercentageFromMaxHp = (float)damageChanged.Character.StatusManager.CurrentHp / damageChanged.Character.StatusManager.MaxHp;
                        damageTextSpawner.ShowDamage(damageChanged.Character.CurrentPosition, damageChanged.Message.Damage, damagePercentageFromMaxHp, Settings.DamageTextDisplayTime.Value);
                        if (damagePercentageFromMaxHp > 0.25f || hpPercentageFromMaxHp < 0.25f)
                        {
                            flushController.Flush(Settings.FlushDuration.Value);
                        }
                    }
                }
            );
            map.CharacterManager.PlayerEvents.OnHealReceived.Subscribe(
                healChanged =>
                {
                    if (healChanged.Character.Visibility.CurrentValue == true)
                    {
                        damageTextSpawner.ShowHeal(healChanged.Character.CurrentPosition, healChanged.Message.Heal, (float)healChanged.Message.Heal / healChanged.Character.StatusManager.MaxHp, Settings.DamageTextDisplayTime.Value);
                    }
                }
            );
        });
    }
}

