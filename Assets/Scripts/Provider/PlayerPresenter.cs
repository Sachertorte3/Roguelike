#nullable enable
using Model.Characters;
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
        public PlayerPresenter(CharacterManager characterManager, SynchronizedCharacterView characters,
            SynchronizedItemView _, InventoryView inventoryView, StatLine statLine, CameraFollowTarget camera)
        {
            var playerView = characters.Get(characterManager.Player);

            var arrowPrefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Arrow.prefab")
                .WaitForCompletion();
            var arrow = Object.Instantiate(arrowPrefab, playerView.transform);
            arrow.GetComponent<CharacterArrow>().Constract(playerView);

            characterManager.Player.Inventory.OnItemChanged.Subscribe(itemChanged =>
            {
                if (itemChanged.NewValue != null)
                    inventoryView.Replace(itemChanged.NewValue.Icon, itemChanged.NewValue.RemainingUses.CurrentValue,
                        itemChanged.NewValue.Info, itemChanged.Index);
                else
                    inventoryView.Remove(itemChanged.Index);
            });
            characterManager.Player.Inventory.OnItemUpdated.Subscribe(itemUpdated =>
            {
                inventoryView.UpdateCount(itemUpdated.Item.RemainingUses.CurrentValue, itemUpdated.Index);
            });

            Observable.Merge(characterManager.Player.Stats.HpValue, characterManager.Player.Stats.MaxHp)
                .Subscribe(_ => statLine.SetValue(characterManager.Player.Stats.MaxHp.CurrentValue, characterManager.Player.Stats.HpValue.CurrentValue));

            camera.SetTarget(playerView.gameObject);
        }
    }
}