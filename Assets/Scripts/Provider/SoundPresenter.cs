using System;
using Domain.Model;
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
        public SoundPresenter(GameManager gameManager, World world, BGMManager bgmManager, SEManager seManager)
        {
            Settings.GlobalSettings.BGMVolume.Value.SubscribeIncludingCurrentValue(volume => bgmManager.SetVolume(volume / 100f));
            Settings.GlobalSettings.SEVolume.Value.SubscribeIncludingCurrentValue(volume => seManager.SetVolume(volume / 100f));
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

                    _disposable.Add(map.Characters.SubscribeIncludingCurrentObservables(character => character.OnPickUpItem,
                        (character, itemChanged) => { seManager.PickupSE(); }
                    ));
                    _disposable.Add(map.OnEffectSpawned.Subscribe(effectSpawned => { seManager.AttackSE(); }));
                    _disposable.Add(map.Player.Character.Entity.OnTeleport.Subscribe(teleport => { seManager.TeleportSE(); }));
                },
                _ => _disposable.Clear());
            gameManager.OnPlaySE.Subscribe(se =>
            {
                switch (se)
                {
                    case SE.GrassWalk:
                        seManager.GrassWalkSE();
                        break;
                    case SE.Attack:
                        seManager.AttackSE();
                        break;
                    case SE.Pickup:
                        seManager.PickupSE();
                        break;
                    case SE.Stairs:
                        seManager.StairsSE();
                        break;
                    case SE.Teleport:
                        seManager.TeleportSE();
                        break;
                    default:
                        throw new NotImplementedException($"SE {se} is not implemented");
                }
            });
        }
    }
}