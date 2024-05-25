#nullable enable
using Model;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using View;
using View.UI;

namespace Provider
{
    public class PlayerPresenter
    {
        [Inject]
        public PlayerPresenter(World world, SynchronizedCharacterView characters, SynchronizedItemView _, StatLine statLine)
        {
            var playerView = characters.Get(world.Player);

            var arrowPrefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Arrow.prefab")
                .WaitForCompletion();
            var arrow = Object.Instantiate(arrowPrefab, playerView.transform);
            arrow.GetComponent<CharacterArrow>().Constract(playerView);

            Observable.Merge(world.Player.StatusManager.Stats.HpValue, world.Player.StatusManager.Stats.MaxHp)
                .Subscribe(_ => statLine.SetValue(world.Player.StatusManager.MaxHp, world.Player.StatusManager.CurrentHp));
        }
    }
}