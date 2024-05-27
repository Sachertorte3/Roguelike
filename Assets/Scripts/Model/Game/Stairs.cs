using Model.Domain.Entities;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Model.Game
{
    public class Stairs : IEventEntity
    {
        private Entity _entity;

        public Stairs(Vector2Int position)
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
            .LoadAssetAsync<Sprite>("Assets/Images/MapChipPalettes/Tiles/tiles.png[tiles_42]").WaitForCompletion();

        public void DoEvent()
        {
            Globals.GameManager.LoadMap();
        }

        public void SetVisiblity(bool visiblity)
        {
            _entity.SetVisibility(visiblity);
        }
    }
}