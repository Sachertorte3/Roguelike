using System;
using Data.Map;
using Model.Domain.Entities;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Model.Game
{
    public class DownStairs : IDisposable, IEventEntity
    {
        private Entity _entity;

        public DownStairs(Vector2Int position)
        {
            _entity = new Entity(position);
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

        public void DoEvent()
        {
            Globals.GameManager.LoadNewMap();
        }

        public void SetVisiblity(bool visiblity)
        {
            _entity.SetVisibility(visiblity);
        }
    }
    public class UpStairs : IDisposable, IEventEntity
    {
        private Entity _entity;
        private TilemapMemento _tilemap;

        public UpStairs(Vector2Int position, TilemapMemento tilemap)
        {
            _entity = new Entity(position);
            _tilemap = tilemap;
        }

        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public Entity Entity => _entity;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;

        public Sprite Icon => Addressables
            .LoadAssetAsync<Sprite>("MapChip/(Base)BaseChip_pipo.png[(Base)BaseChip_pipo_343]").WaitForCompletion();
        ~UpStairs()
        {
            Dispose();
        }
        public void Dispose()
        {
            _entity.Dispose();
        }

        public void DoEvent()
        {
            Globals.GameManager.LoadMap(_tilemap);
        }

        public void SetVisiblity(bool visiblity)
        {
            _entity.SetVisibility(visiblity);
        }
    }
}