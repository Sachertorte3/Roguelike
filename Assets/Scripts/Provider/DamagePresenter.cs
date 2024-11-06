using Domain.Model.Setting;
using Game;
using R3;
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
            world.ActiveMap.SubscribeToAllItemsIgnoreNull(map =>
                {
                    damageTextSpawner.DeleteAllText();
                    _disposable.Add(map.Player.StatusManager.OnDamageReceived.Subscribe(damageChanged =>
                        {
                            var damagePercentageFromMaxHp = damageChanged * 100 /
                                                            map.Player.StatusManager.Stats.MaxHp.CurrentValue;
                            var hpPercentageFromMaxHp = map.Player.StatusManager.Stats.HpValue.CurrentValue *
                            100 / map.Player.StatusManager.Stats.MaxHp.CurrentValue;
                            if (damagePercentageFromMaxHp > Settings.SignificantDamageThresholdPercentage.Value ||
                                hpPercentageFromMaxHp < Settings.LowHpThresholdPercentage.Value)
                            {
                                flushController.Flush(Settings.FlushDuration.Value);
                            }
                        }
                    ));
                    _disposable.Add(map.Characters.SubscribeToAllObservables(
                        character => character.StatusManager.OnDamageReceived,
                        (character, damageChanged) =>
                        {
                            if (character.Entity.Visibility.CurrentValue)
                            {
                                var damagePercentageFromMaxHp = damageChanged * 100 /
                                                                character.StatusManager.Stats.MaxHp.CurrentValue;
                                damageTextSpawner.ShowDamage(character.Entity.CurrentPosition,
                                    damageChanged, damagePercentageFromMaxHp,
                                    Settings.DamageTextDisplayTime.Value);
                            }
                        }
                    ));
                    _disposable.Add(map.Characters.SubscribeToAllObservables(
                        character => character.StatusManager.OnHealReceived,
                        (character, healChanged) =>
                        {
                            if (character.Entity.Visibility.CurrentValue)
                            {
                                var healPercentageFromMaxHp = healChanged * 100 /
                                                          character.StatusManager.Stats.MaxHp.CurrentValue;
                                damageTextSpawner.ShowHeal(character.Entity.CurrentPosition, healChanged,
                                    healPercentageFromMaxHp, Settings.DamageTextDisplayTime.Value);
                            }
                        }
                    ));
                },
                _ => _disposable.Clear());
        }
    }
}