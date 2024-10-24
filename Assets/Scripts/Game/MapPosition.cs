#nullable enable
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Map;
using Unity.Logging;
using UnityEngine;

namespace Game
{
    public class MapPosition : IMapPosition
    {
        public Vector2Int Position { get; init; }
        public IMap Map { get; init; }
        public ITilemapViewer TilemapViewer { get; init; }
        public MapPosition(Vector2Int position, IMap map, ITilemapViewer tilemapViewer)
        {
            Position = position;
            Map = map;
            TilemapViewer = tilemapViewer;
        }
        public bool IsOverlapped(params EntityLayer[] layers)
        {
            return Map.Entities.On(layers).Count(entity => entity.CurrentPosition == Position) > 1;
        }

        public bool IsBlankIgnoreWall(params EntityLayer[] layers)
        {
            if (layers.Any())
                return !Map.Entities.On(layers).At(Position).Any();
            return !Map.Entities.At(Position).Any();
        }

        public bool IsBlank(params EntityLayer[] layers)
        {
            return IsPassableOnMap() && IsBlankIgnoreWall(layers);
        }

        public bool IsBlankAndStandable(params EntityLayer[] layers)
        {
            return IsWalkableOnMap() && IsBlankIgnoreWall(layers);
        }

        public bool CanPlace(bool isFlying, bool canThroughWalls, bool ignoreEntity,
            params EntityLayer[] layers)
        {
            if (!layers.Any())
                Log.Warning("No layers specified for CanPlace");

            return (ignoreEntity, canThroughWalls, isFlying) switch
            {
                (true, true, _) => Map.IsInside(Position),
                (true, false, true) => IsPassableOnMap(),
                (true, false, false) => IsWalkableOnMap(),
                (false, true, _) => Map.IsInside(Position) && IsBlankIgnoreWall(layers),
                (false, false, true) => IsBlank(layers),
                (false, false, false) => IsBlankAndStandable(layers)
            };
        }

        public bool IsWalkable(IAffiliation actor)
        {
            if (!IsWalkableOnMap())
                return false;
            var entity = Map.Entities.On(EntityLayer.Middle).At(Position).FirstOrDefault();
            if (entity == null)
                return true;
            if (entity is ICharacter character && character != Map.Player)
                return !character.Affiliation.IsEnemy(actor);
            return false;
        }

        public bool IsWalkableOnMap()
        {
            return TilemapViewer.IsWalkable(Position);
        }

        public bool IsPassableOnMap()
        {
            return TilemapViewer.IsPassable(Position);
        }

        public bool IsLightPassable()
        {
            return TilemapViewer.IsTransparent(Position);
        }
    }
}