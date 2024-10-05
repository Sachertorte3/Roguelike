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
            Settings.BGMVolume.SubscribeToAll(volume => bgmManager.SetVolume(volume / 100f));
            Settings.SEVolume.SubscribeToAll(volume => seManager.SetVolume(volume / 100f));
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
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