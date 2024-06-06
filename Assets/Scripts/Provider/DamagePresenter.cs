using Data.Setting;
using Model.Game;
using R3;
using Utilities;
using VContainer;
using View;

public class DamagePresenter
{
    [Inject]
    public DamagePresenter(World world, DamageTextSpawner damageTextSpawner, FlushController flushController)
    {
        world.ActiveMap.SubscribeToAllIgnoreNull(map =>
        {
            map.CharacterManager.PlayerEvents.OnDamageReceived.Subscribe(damageChanged =>
            {
                var damagePercentageFromMaxHp = damageChanged.Message.Damage * 100 / damageChanged.Character.StatusManager.Stats.MaxHp.CurrentValue;
                var hpPercentageFromMaxHp = damageChanged.Character.StatusManager.Stats.HpValue.CurrentValue * 100 / damageChanged.Character.StatusManager.Stats.MaxHp.CurrentValue;
                if (damagePercentageFromMaxHp > Settings.SignificantDamageThresholdPercentage.Value || hpPercentageFromMaxHp < Settings.LowHpThresholdPercentage.Value)
                {
                    flushController.Flush(Settings.FlushDuration.Value);
                }
            });
            map.CharacterManager.CharacterEvents.OnDamageReceived.Subscribe(damageChanged =>
            {
                if (damageChanged.Character.Visibility.CurrentValue == true)
                {
                    var damagePercentageFromMaxHp = damageChanged.Message.Damage * 100 / damageChanged.Character.StatusManager.Stats.MaxHp.CurrentValue;
                    damageTextSpawner.ShowDamage(damageChanged.Character.CurrentPosition, damageChanged.Message.Damage, damagePercentageFromMaxHp, Settings.DamageTextDisplayTime.Value);
                }
            });
            map.CharacterManager.CharacterEvents.OnHealReceived.Subscribe(healChanged =>
            {
                if (healChanged.Character.Visibility.CurrentValue == true)
                {
                    var healPercentageFromMaxHp = healChanged.Message.Heal * 100 / healChanged.Character.StatusManager.Stats.MaxHp.CurrentValue;
                    damageTextSpawner.ShowHeal(healChanged.Character.CurrentPosition, healChanged.Message.Heal, healPercentageFromMaxHp, Settings.DamageTextDisplayTime.Value);
                }
            });
        });
    }
}