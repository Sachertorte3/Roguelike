using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Entities;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Domain.Service.Events
{
    public class Stairs : IDisposable, ISerializable<StairsMemento>, IIconEventEntity, IMovementEntity
    {
        public MovementEntityType Type { get; init; }
        public Location Destination { get; init; }
        private readonly Entity _entity;
        public Id<IEntity> DestinationId { get; init; }
        public ReadOnlyReactiveProperty<bool> IsLocked { get; private set; }
        public Stairs(StairsMemento data, ReadOnlyReactiveProperty<bool> isLocked)
        {
            Type = data.Type;
            _entity = new Entity(data.Entity);
            Destination = data.Destination;
            DestinationId = new Id<IEntity>(data.DestinationId);
            IsLocked = isLocked;
        }

        public void Dispose()
        {
            _entity.Dispose();
        }

        public Id<IEntity> Id => _entity.Id;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;

        public Sprite Icon => Type switch
        {
            MovementEntityType.UpStairs => Addressables
                .LoadAssetAsync<Sprite>("MapChip/(Base)BaseChip_pipo.png[(Base)BaseChip_pipo_342]").WaitForCompletion(),
            MovementEntityType.DownStairs => Addressables
                .LoadAssetAsync<Sprite>("MapChip/(Base)BaseChip_pipo.png[(Base)BaseChip_pipo_334]").WaitForCompletion(),
            _ => throw new NotImplementedException()
        };

        public EventTrigger Trigger => EventTrigger.Tread;
        public Observable<Unit> OnDestroyed => _entity.OnDestroyed;
        public bool CanExecuteEvent => !IsLocked.CurrentValue;

        public async UniTask DoEvent(IGameManager gameManager, IMapManager mapManager)
        {
            if (await gameManager.GetChoice("階段を見つけた", "進む", "やめる") == 0)
            {
                gameManager.LoadMap(Destination, DestinationId);
            }
        }

        public void SetVisibility(bool visibility)
        {
            _entity.SetVisibility(visibility);
        }

        public void Destroy()
        {
            _entity.Destroy();
        }

        public StairsMemento Serialize()
        {
            return new StairsMemento
            {
                Type = Type,
                Destination = Destination,
                Entity = _entity.Serialize(),
                DestinationId = DestinationId.ToString()
            };
        }

        public static StairsMemento Build(MovementEntityType type, Vector2Int position, Id<IEntity> id, Location destination, Id<IEntity> destinationId)
        {
            return new StairsMemento
            {
                Type = type,
                Destination = destination,
                Entity = Entity.Build(id, position, EntityLayer.Bottom),
                DestinationId = destinationId.ToString()
            };
        }

        public static StairsMemento Build(MovementEntityType type, Vector2Int position, Location destination)
        {
            return Build(type, position, Id<IEntity>.Generate(), destination, Id<IEntity>.Generate());
        }

        ~Stairs()
        {
            Dispose();
        }
    }
}