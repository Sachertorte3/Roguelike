#nullable enable
using BidirectionalMap;
using R3;
using Scripts.Model.Characters;
using Scripts.Model.Items;
using Scripts.Utilities;
using Scripts.View;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scripts.Provider
{
    public class SynchronizedItemView
    {
        private BiMap<ItemEntity, SpriteView> itemViewDict = new();
        private Func<HashSet<Vector2Int>> _getVisibleArea;
        private GameObject _itemViewPrefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/ItemView.prefab").WaitForCompletion();
        public SynchronizedItemView(ItemManager itemManager, CharacterManager characterManager)
        {
            _getVisibleArea = characterManager.Player.Area.Get;

            itemManager.OnItemAdded.Subscribe(item =>
            {
                Add(item);
            });
            itemManager.OnItemRemoved.Subscribe(item =>
            {
                Remove(item);
            });
        }
        public void Add(ItemEntity item)
        {
            SpriteView spriteView = GameObject.Instantiate<GameObject>(_itemViewPrefab).GetComponent<SpriteView>();
            ObjectsManager.RegisterComponent(spriteView.GetComponent<SpriteView>());
            spriteView.transform.position = (Vector3Int)item.CurrentPosition;
            spriteView.GetComponent<SpriteRenderer>().sprite = item.Item.Icon;
            spriteView.SetVisibility(_getVisibleArea().Contains(item.CurrentPosition));
            itemViewDict.Add(item, spriteView);
        }
        public void Remove(ItemEntity item)
        {
            GameObject.Destroy(itemViewDict.Forward[item].gameObject);
            itemViewDict.Remove(item);
        }
        public ItemEntity Get(SpriteView view) => itemViewDict.Reverse[view];
        public SpriteView Get(ItemEntity item) => itemViewDict.Forward[item];
    }
}