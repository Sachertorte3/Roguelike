#nullable enable
using Scripts.Model.Characters;
using Scripts.View;
using Scripts.View.UI;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using R3;
using Scripts.Model.Items;
using System;

namespace Scripts.Provider
{
    public class PlayerPresenter
    {
        [Inject]
        public PlayerPresenter(CharacterManager characterManager, SynchronizedCharacterView characters, SynchronizedItemView _, InventoryView inventoryView, CameraFollowTarget camera)
        {
            CharacterView playerView = characters.Get(characterManager.Player);

            GameObject arrowPrefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Arrow.prefab").WaitForCompletion();
            GameObject arrow = GameObject.Instantiate(arrowPrefab, playerView.transform);
            arrow.GetComponent<CharacterArrow>().Constract(playerView);

            characterManager.Player.Inventory.OnItemChanged.Subscribe(itemChanged =>
            {
                if (itemChanged.NewValue != null)
                {
                    inventoryView.Replace(itemChanged.NewValue.Icon, itemChanged.NewValue.RemainingUses.CurrentValue, itemChanged.Index);
                }
                else
                {
                    inventoryView.Remove(itemChanged.Index);
                }
            });
            characterManager.Player.Inventory.OnItemUpdated.Subscribe(itemUpdated =>
            {
                inventoryView.UpdateCount(itemUpdated.Item.RemainingUses.CurrentValue, itemUpdated.Index);
            });

            camera.SetTarget(playerView.gameObject);
        }
    }
}