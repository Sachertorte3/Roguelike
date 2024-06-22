using Data;
using Data.Character;
using Data.Map;
using Model.Domain.Entities;
using Model.Domain.Items;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Model.Domain.Events
{
    public class Chest : ISerializable<ChestMemento>, IEventEntity
    {
        private Entity _entity;
        public Sprite Icon => Addressables.LoadAssetAsync<Sprite>("Assets/Images/Monsters/ChestA.png[Chest_0]").WaitForCompletion();
        public EventTrigger Trigger => EventTrigger.Touch;
        private ItemData _item;

        public Entity Entity => _entity;

        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;

        public Vector2Int CurrentPosition => _entity.CurrentPosition;

        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;

        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;

        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;
        public static ChestMemento Build(Vector2Int position, ItemData item) => new(item, new EntityMemento(position, EntityLayer.Middle));
        public Chest(ChestMemento memento)
        {
            _item = memento.Item;
            _entity = new Entity(memento.Entity);
        }
        public ChestMemento Serialize()
        {
            return new(_item, _entity.Serialize());
        }
        public void DoEvent(IGameManager gameManager, IMapManager mapManager)
        {
            mapManager.SpawnItem(new Item(_item), CurrentPosition);
            mapManager.RemoveEventEntity(this);
        }

        public void Dispose()
        {
            _entity.Dispose();
        }
    }
}