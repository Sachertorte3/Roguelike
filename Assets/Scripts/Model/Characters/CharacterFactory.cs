#nullable enable
using Model.Characters.Behavior;
using R3;
using UnityEngine;

namespace Model.Characters
{
    internal sealed class CharacterFactory
    {
        public Character CreateCharacter(Vector2Int spawnPosition, ICharacterBehavior behavior,
            ReactiveProperty<bool> canIgnoreWall)
        {
            return new Character(spawnPosition, behavior, canIgnoreWall);
        }
    }
}