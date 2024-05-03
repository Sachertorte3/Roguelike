#nullable enable
using R3;
using Scripts.Model.Characters.Behavior;
using UnityEngine;

namespace Scripts.Model.Characters
{
    internal sealed class CharacterFactory
    {
        public Character CreateCharacter(Vector2Int spawnPosition, ICharacterBehavior behavior, ReactiveProperty<bool> canIgnoreWall)
        {
            return new Character(spawnPosition, behavior, canIgnoreWall);
        }
    }
}
