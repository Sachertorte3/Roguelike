#nullable enable
using BidirectionalMap;
using R3;
using Scripts.Model.Characters;
using Scripts.Model.Items;
using Scripts.Model.Setting;
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
        private BiMap<ItemEntity, EntityView> itemViewDict = new();
        private InputReceiver _inputReceiver;
        private Func<HashSet<Vector2Int>> _getVisibleArea;
        private GameObject _itemViewPrefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/ItemView.prefab").WaitForCompletion();
        public SynchronizedItemView(InputReceiver inputReceiver, ItemManager itemManager, CharacterManager characterManager)
        {
            _inputReceiver = inputReceiver;
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
            EntityView entityView = GameObject.Instantiate<GameObject>(_itemViewPrefab).GetComponent<EntityView>();
            SpriteView spriteView = entityView.GetComponent<SpriteView>();
            ObjectsManager.RegisterComponent(spriteView.GetComponent<SpriteView>());
            entityView.Construct(_inputReceiver);
            item.OnMove.Subscribe(move => entityView.Move(move.destination, move.direction));
            Settings.MoveMilliseconds.Subscribe(value => entityView.MoveMilliseconds = value);
            Settings.DashMilliseconds.Subscribe(value => entityView.DashMilliseconds = value);
            spriteView.transform.position = (Vector3Int)item.CurrentPosition;
            spriteView.GetComponent<SpriteRenderer>().sprite = item.Item.Icon;
            spriteView.SetVisibility(_getVisibleArea().Contains(item.CurrentPosition));
            itemViewDict.Add(item, entityView);
        }
        public void Remove(ItemEntity item)
        {
            GameObject.Destroy(itemViewDict.Forward[item].gameObject);
            itemViewDict.Remove(item);
        }
        public ItemEntity Get(EntityView view) => itemViewDict.Reverse[view];
        public EntityView Get(ItemEntity item) => itemViewDict.Forward[item];
    }
}