#nullable enable
using Domain.Model.Setting;
using Game;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
            StatLine statLine)
        {
            CompositeDisposable _disposable = new();
            world.ActiveMap.SubscribeToAllItemsIgnoreNull(map =>
                {
                    if (map.Player.Character.IsDead)
                    {
                        return;
                    }

                    var playerView = characters.Get(map.Player.Character);

                    var arrowPrefab = ScriptableObjectLoader.LoadPrefab("Arrow");
                    var arrow = Object.Instantiate(arrowPrefab, playerView.transform);
                    arrow.GetComponent<CharacterArrow>().SetCharacter(playerView);

                    _disposable.Add(Observable
                        .Merge(map.Player.Character.Status.Stats.HpValue, map.Player.Character.Status.Stats.MaxHp)
                        .Subscribe(_ =>
                        {
                            var hpPercentageFromMaxHp = map.Player.Character.Status.Stats.HpValue.CurrentValue * 100 /
                                                        map.Player.Character.Status.Stats.MaxHp.CurrentValue;
                            statLine.SetValue(map.Player.Character.Status.Stats.MaxHp.CurrentValue,
                                map.Player.Character.Status.Stats.HpValue.CurrentValue);
                            if (hpPercentageFromMaxHp < Settings.LowHpThresholdPercentage.Value)
                            {
                                statLine.SetTextColor(Color.red);
                            }
                            else
                            {
                                statLine.SetTextColor(Color.white);
                            }
                        }));
                },
                map => { _disposable.Clear(); });
        }
    }
}