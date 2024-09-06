using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Service.Entities;
using Domain.Service.Items;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Domain.Service.Events
{
    public class Chest : ISerializable<ChestMemento>, IIconEventEntity
    {
        private Entity _entity;
        private Item _item;

        public Chest(ChestMemento memento)
        {
            _item = new Item(memento.Item);
            _entity = new Entity(memento.Entity);
        }

        public Sprite Icon => Addressables.LoadAssetAsync<Sprite>("Assets/Images/Monsters/ChestA.png[Chest_0]")
            .WaitForCompletion();

        public EventTrigger Trigger => EventTrigger.Touch;
        public Id<IEntity> Id => _entity.Id;
        public Observable<Unit> OnDestroyed => _entity.OnDestroyed;
        public bool CanExecuteEvent => true;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;

        public UniTask DoEvent(IGameManager gameManager, IMapManager mapManager)
        {
            mapManager.SpawnItem(_item, CurrentPosition);
            mapManager.RemoveEventEntity(this);
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            _entity.Dispose();
        }

        public void SetVisibility(bool visibility)
        {
            _entity.SetVisibility(visibility);
        }

        public void Destroy()
        {
            _entity.Destroy();
        }

        public ChestMemento Serialize()
        {
            return new ChestMemento
            {
                Item = _item.Serialize(),
                Entity = _entity.Serialize()
            };
        }

        public static ChestMemento Build(Vector2Int position, ItemData item)
        {
            return new ChestMemento
            {
                Item = new Item(item).Serialize(),
                Entity = Entity.Build(position, EntityLayer.Middle)
            };
        }
    }
}