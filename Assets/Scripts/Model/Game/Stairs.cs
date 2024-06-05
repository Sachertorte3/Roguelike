using System;
using Data.Map;
using Model.Domain.Entities;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Model.Game
{
    public class DownStairs : IDisposable, ISerializable<DownStairsMemento>, IEventEntity
    {
        private int _destinationMapId;
        private Entity _entity;

        public static DownStairsMemento Build(Vector2Int position, int destinationMapId)
        {
            return new DownStairsMemento(
                destinationMapId,
                Entity.Build(position)
            );
        }
        public DownStairs(DownStairsMemento data)
        {
            _entity = new Entity(data.Entity);
            _destinationMapId = data.DestinationMapId;
        }

        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public Entity Entity => _entity;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;

        public Sprite Icon => Addressables
            .LoadAssetAsync<Sprite>("MapChip/(Base)BaseChip_pipo.png[(Base)BaseChip_pipo_334]").WaitForCompletion();
        ~DownStairs()
        {
            Dispose();
        }
        public void Dispose()
        {
            _entity.Dispose();
        }
        public DownStairsMemento Serialize()
        {
            return new DownStairsMemento(
                _destinationMapId,
                _entity.Serialize()
            );
        }
        public void DoEvent()
        {
            Globals.GameManager.LoadMap(_destinationMapId);
        }

        public void SetVisiblity(bool visiblity)
        {
            _entity.SetVisibility(visiblity);
        }
    }
    public class UpStairs : IDisposable, ISerializable<UpStairsMemento>, IEventEntity
    {
        private int _destinationMapId;
        private Entity _entity;

        public static UpStairsMemento Build(Vector2Int position, int destinationMapId)
        {
            return new UpStairsMemento(
                destinationMapId,
                Entity.Build(position)
            );
        }
        public UpStairs(UpStairsMemento data)
        {
            _entity = new Entity(data.Entity);
            _destinationMapId = data.DestinationMapId;
        }

        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public Entity Entity => _entity;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;

        public Sprite Icon => Addressables
            .LoadAssetAsync<Sprite>("MapChip/(Base)BaseChip_pipo.png[(Base)BaseChip_pipo_342]").WaitForCompletion();
        ~UpStairs()
        {
            Dispose();
        }
        public void Dispose()
        {
            _entity.Dispose();
        }
        public UpStairsMemento Serialize()
        {
            return new UpStairsMemento(
                _destinationMapId,
                _entity.Serialize()
            );
        }

        public void DoEvent()
        {
            Globals.GameManager.LoadMap(_destinationMapId);
        }

        public void SetVisiblity(bool visiblity)
        {
            _entity.SetVisibility(visiblity);
        }
    }
}