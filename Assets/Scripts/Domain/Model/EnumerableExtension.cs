using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;
using R3;
using UnityEngine;

namespace Domain.Model
{
    public static class GameEnumerableExtension
    {
        public static IEnumerable<IMapPosition> In(this IEnumerable<IMapPosition> ie, IEnumerable<Vector2Int> area)
        {
            return ie.Where(position => area.Contains(position.Position));
        }
        public static IEnumerable<Vector2Int> Values(this IEnumerable<IMapPosition> ie)
        {
            return ie.Select(position => position.Position);
        }
        public static IEnumerable<T> In<T>(this IEnumerable<T> ie, IEnumerable<Vector2Int> area) where T : IEntity
        {
            return ie.Where(item => area.Contains(item.CurrentPosition));
        }
        public static IEnumerable<T> On<T>(this IEnumerable<T> ie, params EntityLayer[] layers) where T : IEntity
        {
            return ie.Where(item => layers.Contains(item.Layer));
        }
        public static IEnumerable<T> At<T>(this IEnumerable<T> ie, Vector2Int position) where T : IEntity
        {
            return ie.Where(item => item.CurrentPosition == position);
        }
        public static IEnumerable<T> FromAffiliation<T>(this IEnumerable<T> ie, IHasAffiliation viewer, AffiliationType type) where T : ICharacter
        {
            return ie.Where(item => viewer.Affiliation.GetAffiliationType(item.Affiliation) == type);
        }
        public static IEnumerable<T> IsVisible<T>(this IEnumerable<T> ie, Vector2Int position) where T : ICharacter
        {
            return ie.Where(item => item.VisionRange.IsVisible(position));
        }
        public static IEnumerable<Vector2Int> Positions<T>(this IEnumerable<T> ie) where T : IEntity
        {
            return ie.Select(item => item.CurrentPosition);
        }
    }
}