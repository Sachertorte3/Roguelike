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
    public class DownStairs : IDisposable, ISerializable<DownStairsMemento>, IIconEventEntity
    {
        private int _destinationLevel;
        private Entity _entity;
        public ReadOnlyReactiveProperty<bool> IsLocked { get; private set; }

        public DownStairs(DownStairsMemento data, ReadOnlyReactiveProperty<bool> isLocked)
        {
            _entity = new Entity(data.Entity);
            _destinationLevel = data.DestinationLevel;
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

        public Sprite Icon => Addressables
            .LoadAssetAsync<Sprite>("MapChip/(Base)BaseChip_pipo.png[(Base)BaseChip_pipo_334]").WaitForCompletion();

        public EventTrigger Trigger => EventTrigger.Tread;
        public Observable<Unit> OnDestroyed => _entity.OnDestroyed;
        public bool CanExecuteEvent => !IsLocked.CurrentValue;

        public async UniTask DoEvent(IGameManager gameManager, IMapManager mapManager)
        {
            var choice = await gameManager.GetChoice("階段を見つけた", "下る", "やめる");
            if (choice == 0)
            {
                gameManager.LoadMap(new Location("Dungeon", _destinationLevel));
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

        public DownStairsMemento Serialize()
        {
            return new DownStairsMemento
            {
                DestinationLevel = _destinationLevel,
                Entity = _entity.Serialize()
            };
        }

        public static DownStairsMemento Build(Vector2Int position, int level)
        {
            return new DownStairsMemento
            {
                DestinationLevel = level,
                Entity = Entity.Build(position, EntityLayer.Bottom)
            };
        }

        ~DownStairs()
        {
            Dispose();
        }
    }
}