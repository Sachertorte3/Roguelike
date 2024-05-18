#nullable enable
using Model;
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
        public PlayerPresenter(World world, SynchronizedCharacterView characters,
            SynchronizedItemView _, InventoryView inventoryView, StatLine statLine, CameraFollowTarget camera)
        {
            var playerView = characters.Get(world.Player);

            var arrowPrefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Arrow.prefab")
                .WaitForCompletion();
            var arrow = Object.Instantiate(arrowPrefab, playerView.transform);
            arrow.GetComponent<CharacterArrow>().Constract(playerView);

            world.Player.Inventory.OnItemChanged.Subscribe(itemChanged =>
            {
                if (itemChanged.NewValue != null)
                    inventoryView.Replace(itemChanged.NewValue.Icon, itemChanged.NewValue.RemainingUses.CurrentValue,
                        itemChanged.NewValue.Info, itemChanged.Index);
                else
                    inventoryView.Remove(itemChanged.Index);
            });
            world.Player.Inventory.OnItemUpdated.Subscribe(itemUpdated =>
            {
                inventoryView.UpdateCount(itemUpdated.Item.RemainingUses.CurrentValue, itemUpdated.Index);
            });

            Observable.Merge(world.Player.Stats.HpValue, world.Player.Stats.MaxHp)
                .Subscribe(_ => statLine.SetValue(world.Player.Stats.MaxHp.CurrentValue, world.Player.Stats.HpValue.CurrentValue));

            camera.SetTarget(playerView.gameObject);
        }
    }
}