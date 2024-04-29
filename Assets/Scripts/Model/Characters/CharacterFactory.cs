#nullable enable
using Scripts.Model.Characters.Behavior;
using UnityEngine;

namespace Scripts.Model.Characters
{
    internal sealed class CharacterFactory
    {
        public Character CreateCharacter(Vector2Int spawnPosition, ICharacterBehavior behavior, World world)
        {
            return new Character(spawnPosition, behavior, world);
        }
    }
}
