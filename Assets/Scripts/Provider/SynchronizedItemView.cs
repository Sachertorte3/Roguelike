#nullable enable
using System;
using System.Collections.Generic;
using BidirectionalMap;
using Model.Characters;
using Model.Items;
using Model.Setting;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities.ObjectsManager;
using View;
using Object = UnityEngine.Object;

namespace Provider
{
    public class SynchronizedItemView
    {
        private readonly EffectViewSpawner _effectViewSpawner;
        private Func<HashSet<Vector2Int>> _getVisibleArea;
        private readonly InputReceiver _inputReceiver;

        private readonly GameObject _itemViewPrefab =
            Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/ItemView.prefab").WaitForCompletion();

        private readonly BiMap<ItemEntity, EntityView> itemViewDict = new();

        public SynchronizedItemView(EffectViewSpawner effectViewSpawner, InputReceiver inputReceiver,
            ItemManager itemManager, CharacterManager characterManager)
        {
            _effectViewSpawner = effectViewSpawner;
            _inputReceiver = inputReceiver;
            _getVisibleArea = characterManager.Player.Area.Get;

            itemManager.OnItemAdded.Subscribe(item => { Add(item); });
            itemManager.OnItemRemoved.Subscribe(item => { Remove(item); });
        }

        public void Add(ItemEntity item)
        {
            var entityView = Object.Instantiate(_itemViewPrefab).GetComponent<EntityView>();
            entityView.Construct(_inputReceiver);
            item.OnMove.Subscribe(move => entityView.Move(move.destination, move.direction));
            item.OnUseSkill.Subscribe(useSkill =>
                _effectViewSpawner.Spawn(useSkill.skill.GetArea(useSkill.position, useSkill.direction),
                    Settings.EffectDisplayTime.Value));
            Settings.ThrowMilliseconds.Subscribe(value => entityView.MoveMilliseconds = value);
            Settings.ThrowMilliseconds.Subscribe(value => entityView.DashMilliseconds = value);

            var spriteView = entityView.GetComponent<SpriteView>();
            spriteView.GetComponent<SpriteView>().RegisterComponent();
            spriteView.transform.position = (Vector3Int)item.CurrentPosition;
            spriteView.GetComponent<SpriteRenderer>().sprite = item.Item.Icon;
            item.Visibility.Subscribe(visibility => spriteView.SetVisibility(visibility));
            itemViewDict.Add(item, entityView);
        }

        public void Remove(ItemEntity item)
        {
            Object.Destroy(itemViewDict.Forward[item].gameObject);
            itemViewDict.Remove(item);
        }

        public ItemEntity Get(EntityView view)
        {
            return itemViewDict.Reverse[view];
        }

        public EntityView Get(ItemEntity item)
        {
            return itemViewDict.Forward[item];
        }
    }
}