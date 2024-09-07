#nullable enable
using Domain.Model.Setting;
using Model.Game;
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
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
                {
                    if (map.Player.CurrentHp <= 0)
                    {
                        return;
                    }
                    var playerView = characters.Get(map.Player);

                    var arrowPrefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Arrow.prefab")
                        .WaitForCompletion();
                    var arrow = Object.Instantiate(arrowPrefab, playerView.transform);
                    arrow.GetComponent<CharacterArrow>().SetCharacter(playerView);

                    _disposable.Add(Observable.Merge(map.Player.StatusManager.Stats.HpValue, map.Player.StatusManager.Stats.MaxHp)
                        .Subscribe(_ =>
                        {
                            var hpPercentageFromMaxHp = map.Player.StatusManager.Stats.HpValue.CurrentValue * 100 /
                                                        map.Player.StatusManager.Stats.MaxHp.CurrentValue;
                            statLine.SetValue(map.Player.StatusManager.Stats.MaxHp.CurrentValue,
                                map.Player.StatusManager.Stats.HpValue.CurrentValue);
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
                map =>
                {
                    _disposable.Clear();
                });
        }
    }
}