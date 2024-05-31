#nullable enable
using Codice.Client.BaseCommands;
using Model;
using Model.Game;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using View;
using View.UI;
using Utilities;

namespace Provider
{
    public class PlayerPresenter
    {
        [Inject]
        public PlayerPresenter(World world, SynchronizedCharacterView characters, SynchronizedItemView _,
            StatLine statLine)
        {
            GameObject arrow = null;
            world.ActiveMap.SubscribeToAll(map =>
            {
                var playerView = characters.Get(map.Player);

                var arrowPrefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Arrow.prefab")
                    .WaitForCompletion();
                var arrow = Object.Instantiate(arrowPrefab, playerView.transform);
                arrow.GetComponent<CharacterArrow>().SetCharacter(playerView);

                Observable.Merge(map.Player.StatusManager.Stats.HpValue, map.Player.StatusManager.Stats.MaxHp)
                    .Subscribe(_ =>
                        statLine.SetValue(map.Player.StatusManager.MaxHp, map.Player.StatusManager.CurrentHp));
            },
            map =>
            {
                Object.Destroy(arrow);
            });
        }
    }
}