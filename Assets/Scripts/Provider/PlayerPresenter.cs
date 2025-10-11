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

                    _disposables.Add(map.Player.Character.Status.Level.Subscribe(level =>
                    {
                        statView.SetLevel(level);
                    }));

                    _disposables.Add(map.Player.Money.Subscribe(money =>
                    {
                        statView.SetMoney(money);
                    }));

                    _disposables.Add(
                        Observable.Merge(
                            map.Player.Character.Inventory.CurrentItemCount.AsUnitObservable(),
                            map.Player.Character.Inventory.Capacity.AsUnitObservable()
                        ).Subscribe(_ =>
                    {
                        var currentItems = map.Player.Character.Inventory.CurrentItemCount.CurrentValue;
                        var capacity = map.Player.Character.Inventory.Capacity.CurrentValue;
                        statView.SetInventory(currentItems, capacity);
                    }));

                    _disposables.Add(Observable
                        .Merge(map.Player.Character.Status.HpValue, map.Player.Character.Status.MaxHp)
                        .Subscribe(_ =>
                        {
                            var hpPercentageFromMaxHp = map.Player.Character.CurrentHp * 100 /
                                                        map.Player.Character.CurrentMaxHp;
                            statView.SetHp(map.Player.Character.CurrentMaxHp,
                                map.Player.Character.CurrentHp);
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