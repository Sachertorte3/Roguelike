#nullable enable
using Scripts.Model.Characters.Behavior;
using UniRx;
using UnityEngine;

namespace Scripts.Model.Characters
{
    internal sealed class CharacterFactory
    {
        public Character CreateCharacter(Vector2Int spawnPosition, ICharacterBehavior behavior, World world, ReactiveProperty<bool> canIgnoreWall)
        {
            return new Character(spawnPosition, behavior, world, canIgnoreWall);
        }
    }
}
