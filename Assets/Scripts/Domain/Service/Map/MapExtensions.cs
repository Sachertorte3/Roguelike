#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Characters;
using UnityEngine;

namespace Domain.Service
{
    public static class MapExtensions
    {
        public static IEnumerable<ICharacter> GetVisibleCharacters(this IMap map, IHasBehavior character)
        {
            return map.GetCharactersInArea(character.VisionRange.VisibleArea);
        }

        public static IEnumerable<ICharacter> GetCharactersCanSeePosition(this IMap map, Vector2Int position)
        {
            return map.Characters.Where(character => character.IsVisible(position));
        }

        public static ICharacter? GetCharacter(this IMap map, int id)
        {
            return map.Characters.FirstOrDefault(character => character.Affiliation.Id == id);
        }
    }
}