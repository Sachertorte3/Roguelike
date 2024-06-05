#nullable enable
using Data.Setting;
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
            GameObject arrow = null;
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
            {
                var playerView = characters.Get(map.Player);

                var arrowPrefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Arrow.prefab")
                    .WaitForCompletion();
                var arrow = Object.Instantiate(arrowPrefab, playerView.transform);
                arrow.GetComponent<CharacterArrow>().SetCharacter(playerView);

                Observable.Merge(map.Player.StatusManager.Stats.HpValue, map.Player.StatusManager.Stats.MaxHp)
                    .Subscribe(_ =>
                    {
                        var hpPercentageFromMaxHp = map.Player.StatusManager.CurrentHp * 100 / map.Player.StatusManager.MaxHp;
                        statLine.SetValue(map.Player.StatusManager.MaxHp, map.Player.StatusManager.CurrentHp);
                        if (hpPercentageFromMaxHp < Settings.LowHpThresholdPercentage.Value)
                        {
                            statLine.SetTextColor(Color.red);
                        }
                        else
                        {
                            statLine.SetTextColor(Color.white);
                        }
                    });
            },
            map =>
            {
                Object.Destroy(arrow);
            });
        }
    }
}