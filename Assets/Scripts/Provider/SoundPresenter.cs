using Domain.Model.Setting;
using Game;
using R3;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class SoundPresenter
    {
        private readonly CompositeDisposable _disposable = new();

        [Inject]
        public SoundPresenter(World world, BGMManager bgmManager, SEManager seManager)
        {
            Settings.BGMVolume.SubscribeIncludingCurrentValue(volume => bgmManager.SetVolume(volume / 100f));
            Settings.SEVolume.SubscribeIncludingCurrentValue(volume => seManager.SetVolume(volume / 100f));
            world.ActiveMap.SubscribeIncludingCurrentValueIgnoreNull(map =>
                {
                    bgmManager.NormalBGM();
                    if (map.IsStolen != null)
                    {
                        _disposable.Add(map.IsStolen.Subscribe(isStolen =>
                        {
                            if (isStolen)
                            {
                                bgmManager.StolenBGM();
                            }
                        }));
                    }

                    _disposable.Add(map.Characters.SubscribeIncludingCurrentObservablesIncludingCurrent(character => character.OnPickUpItem,
                        (character, itemChanged) => { seManager.PickupSE(); }
                    ));
                    _disposable.Add(map.OnEffectSpawned.Subscribe(effectSpawned => { seManager.AttackSE(); }));
                },
                _ => _disposable.Clear());
        }
    }
}