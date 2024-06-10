using Data.Setting;
using Model.Game;
using R3;
using Unity.VisualScripting.YamlDotNet.Core;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class DamagePresenter
    {
        private readonly CompositeDisposable _disposable = new();
        [Inject]
        public DamagePresenter(World world, DamageTextSpawner damageTextSpawner, FlushController flushController)
        {
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
            {
                _disposable.Add(map.CharacterManager.PlayerEvents.OnDamageReceived.Subscribe(damageChanged =>
                {
                    var damagePercentageFromMaxHp = damageChanged.Message.Damage * 100 / damageChanged.Character.StatusManager.Stats.MaxHp.CurrentValue;
                    var hpPercentageFromMaxHp = damageChanged.Character.StatusManager.Stats.HpValue.CurrentValue * 100 / damageChanged.Character.StatusManager.Stats.MaxHp.CurrentValue;
                    if (damagePercentageFromMaxHp > Settings.SignificantDamageThresholdPercentage.Value || hpPercentageFromMaxHp < Settings.LowHpThresholdPercentage.Value)
                    {
                        flushController.Flush(Settings.FlushDuration.Value);
                    }
                }));
                _disposable.Add(map.CharacterManager.CharacterEvents.OnDamageReceived.Subscribe(damageChanged =>
                {
                    if (damageChanged.Character.Visibility.CurrentValue == true)
                    {
                        var damagePercentageFromMaxHp = damageChanged.Message.Damage * 100 / damageChanged.Character.StatusManager.Stats.MaxHp.CurrentValue;
                        damageTextSpawner.ShowDamage(damageChanged.Character.CurrentPosition, damageChanged.Message.Damage, damagePercentageFromMaxHp, Settings.DamageTextDisplayTime.Value);
                    }
                }));
                _disposable.Add(map.CharacterManager.CharacterEvents.OnHealReceived.Subscribe(healChanged =>
                {
                    if (healChanged.Character.Visibility.CurrentValue == true)
                    {
                        var healPercentageFromMaxHp = healChanged.Message.Heal * 100 / healChanged.Character.StatusManager.Stats.MaxHp.CurrentValue;
                        damageTextSpawner.ShowHeal(healChanged.Character.CurrentPosition, healChanged.Message.Heal, healPercentageFromMaxHp, Settings.DamageTextDisplayTime.Value);
                    }
                }));
            },
            _ => _disposable.Clear());
        }
    }
    public class SoundPresenter
    {
        private readonly CompositeDisposable _disposable = new();
        [Inject]
        public SoundPresenter(World world, BGMManager bgmManager, SEManager seManager)
        {
            Settings.BGMVolume.SubscribeToAll(volume => bgmManager.SetVolume(volume/100f));
            Settings.SEVolume.SubscribeToAll(volume => seManager.SetVolume(volume/100f));
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
            {
                _disposable.Add(map.CharacterManager.CharacterEvents.OnPickUpItem.Subscribe(itemChanged =>
                {
                    seManager.PickupSE();
                }));
                _disposable.Add(map.CharacterManager.CharacterEvents.OnEffectSpawned.Subscribe(attackChanged =>
                {
                    seManager.AttackSE();
                }));
            },
            _ => _disposable.Clear());
        }
    }
}