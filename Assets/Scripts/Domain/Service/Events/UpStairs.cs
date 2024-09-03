using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Service.Entities;
using Domain.Model.Memento;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Domain.Service.Events
{
    public class UpStairs : IDisposable, ISerializable<UpStairsMemento>, IIconEventEntity
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

        public Id<IEntity> Id => _entity.Id;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;

        public Sprite Icon => Addressables
            .LoadAssetAsync<Sprite>("MapChip/(Base)BaseChip_pipo.png[(Base)BaseChip_pipo_342]").WaitForCompletion();

        public EventTrigger Trigger => EventTrigger.Tread;
        public Observable<Unit> OnDestroyed => _entity.OnDestroyed;
        public bool CanExecuteEvent => true;

        public async UniTask DoEvent(IGameManager gameManager, IMapManager mapManager)
        {
            if (await gameManager.GetChoice("階段を見つけた", "登る", "やめる") == 0)
            {
                gameManager.LoadMap(_destinationMapId);
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

        public UpStairsMemento Serialize()
        {
            return new UpStairsMemento
            {
                DestinationMapId = _destinationMapId,
                Entity = _entity.Serialize()
            };
        }

        public static UpStairsMemento Build(Vector2Int position, int destinationMapId)
        {
            return new UpStairsMemento
            {
                DestinationMapId = destinationMapId,
                Entity = Entity.Build(position, EntityLayer.Bottom)
            };
        }

        ~UpStairs()
        {
            Dispose();
        }
    }
}