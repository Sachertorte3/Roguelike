#nullable enable
using Domain.Model.Setting;
using Game;
using R3;
using UnityEngine;
using Utilities;
using VContainer;
using View;
using View.UI;

namespace Provider
{
    public class PlayerPresenter
    {
        [Inject]
        public PlayerPresenter(World world, SynchronizedCharacterView characters, SynchronizedItemView _,
            StatView statView)
        {
            CompositeDisposable _disposables = new();
            world.ActiveMap.SubscribeIncludingCurrentValueIgnoreNull(map =>
                {
                    if (map.Player.Character.IsDead)
                    {
                        return;
                    }

                    var playerView = characters.Get(map.Player.Character);

                    var arrowPrefab = ScriptableObjectLoader.LoadPrefab("Arrow");
                    var arrow = Object.Instantiate(arrowPrefab, playerView.transform);
                    arrow.GetComponent<CharacterArrow>().SetCharacter(playerView);

                    _disposables.Add(map.Player.Character.Status.Stats.Level.Subscribe(level =>
                    {
                        statView.SetLevel(level);
                    }));

                    _disposables.Add(Observable
                        .Merge(map.Player.Character.Status.Stats.HpValue, map.Player.Character.Status.Stats.MaxHp)
                        .Subscribe(_ =>
                        {
                            var hpPercentageFromMaxHp = map.Player.Character.Status.Stats.HpValue.CurrentValue * 100 /
                                                        map.Player.Character.Status.Stats.MaxHp.CurrentValue;
                            statView.SetHp(map.Player.Character.Status.Stats.MaxHp.CurrentValue,
                                map.Player.Character.Status.Stats.HpValue.CurrentValue);
                            if (hpPercentageFromMaxHp < Settings.GlobalSettings.LowHpThresholdPercentage.CurrentValue)
                            {
                                statView.SetTextColor(Color.red);
                            }
                            else
                            {
                                statView.SetTextColor(Color.white);
                            }
                        }));
                },
                map => { _disposables.Clear(); });
        }
    }
}