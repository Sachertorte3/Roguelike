using System;
using Data;
using Data.Map;
using Model.Domain.Entities;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Model.Domain.Events
{
    public class DownStairs : IDisposable, ISerializable<DownStairsMemento>, IEventEntity
    {
        private int _destinationMapId;
        private Entity _entity;

        public DownStairs(DownStairsMemento data)
        {
            _entity = new Entity(data.Entity);
            _destinationMapId = data.DestinationMapId;
        }

        public void Dispose()
        {
            _entity.Dispose();
        }

        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public Entity Entity => _entity;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;

        public Sprite Icon => Addressables
            .LoadAssetAsync<Sprite>("MapChip/(Base)BaseChip_pipo.png[(Base)BaseChip_pipo_334]").WaitForCompletion();

        public EventTrigger Trigger => EventTrigger.Tread;

        public void DoEvent(IGameManager gameManager, IMapManager mapManager)
        {
            gameManager.LoadMap(_destinationMapId);
        }

        public void SetVisiblity(bool visiblity)
        {
            _entity.SetVisibility(visiblity);
        }

        public DownStairsMemento Serialize()
        {
            return new DownStairsMemento(
                _destinationMapId,
                _entity.Serialize()
            );
        }

        public static DownStairsMemento Build(Vector2Int position, int destinationMapId)
        {
            return new DownStairsMemento(
                destinationMapId,
                Entity.Build(position, EntityLayer.Bottom)
            );
        }

        ~DownStairs()
        {
            Dispose();
        }
    }

    public class UpStairs : IDisposable, ISerializable<UpStairsMemento>, IEventEntity
    {
        private int _destinationMapId;
        private Entity _entity;

        public UpStairs(UpStairsMemento data)
        {
            _entity = new Entity(data.Entity);
            _destinationMapId = data.DestinationMapId;
        }

        public void Dispose()
        {
            _entity.Dispose();
        }

        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public Entity Entity => _entity;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;

        public Sprite Icon => Addressables
            .LoadAssetAsync<Sprite>("MapChip/(Base)BaseChip_pipo.png[(Base)BaseChip_pipo_342]").WaitForCompletion();

        public EventTrigger Trigger => EventTrigger.Tread;

        public void DoEvent(IGameManager gameManager, IMapManager mapManager)
        {
            gameManager.LoadMap(_destinationMapId);
        }

        public void SetVisiblity(bool visiblity)
        {
            _entity.SetVisibility(visiblity);
        }

        public UpStairsMemento Serialize()
        {
            return new UpStairsMemento(
                _destinationMapId,
                _entity.Serialize()
            );
        }

        public static UpStairsMemento Build(Vector2Int position, int destinationMapId)
        {
            return new UpStairsMemento(
                destinationMapId,
                Entity.Build(position, EntityLayer.Bottom)
            );
        }

        ~UpStairs()
        {
            Dispose();
        }
    }
}