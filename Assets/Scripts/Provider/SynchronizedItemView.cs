#nullable enable
using BidirectionalMap;
using Model;
using Model.Items;
using Model.Setting;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using Utilities.ObjectsManager;
using VContainer;
using View;
using Object = UnityEngine.Object;

namespace Provider
{
    public class SynchronizedItemView
    {
        private readonly EffectViewSpawner _effectViewSpawner;
        private readonly InputReceiver _inputReceiver;

        private readonly GameObject _itemViewPrefab =
            Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/ItemView.prefab").WaitForCompletion();

        private readonly BiMap<ItemEntity, EntityView> itemViewDict = new();

        [Inject]
        public SynchronizedItemView(World world, EffectViewSpawner effectViewSpawner, InputReceiver inputReceiver)
        {
            _effectViewSpawner = effectViewSpawner;
            _inputReceiver = inputReceiver;

            world.Items.SubscribeToAll(Add, Remove);
        }

        public void Add(ItemEntity item)
        {
            var entityView = Object.Instantiate(_itemViewPrefab).GetComponent<EntityView>();
            entityView.Construct(_inputReceiver);
            item.OnMove.Subscribe(move => entityView.Move(move.destination, move.direction)).AddTo(entityView);
            item.OnTeleport.Subscribe(teleport => entityView.Teleport(teleport)).AddTo(entityView);
            item.OnSpawnEffect.Subscribe(useSkill =>
                _effectViewSpawner.Spawn(useSkill, Settings.EffectDisplayTime.Value)).AddTo(entityView);
            Settings.ThrowMilliseconds.Subscribe(value => entityView.MoveMilliseconds = value).AddTo(entityView);
            Settings.ThrowMilliseconds.Subscribe(value => entityView.DashMilliseconds = value).AddTo(entityView);

            var spriteView = entityView.GetComponent<SpriteView>();
            spriteView.GetComponent<SpriteView>().RegisterComponent();
            spriteView.transform.position = (Vector3Int)item.CurrentPosition;
            spriteView.GetComponent<SpriteRenderer>().sprite = item.Item.Icon;
            item.Visibility.Subscribe(visibility => spriteView.SetVisibility(visibility)).AddTo(spriteView);
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