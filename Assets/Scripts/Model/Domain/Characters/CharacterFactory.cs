#nullable enable
using Model.Characters.Behavior;
using Model.Domain;
using R3;
using UnityEngine;

namespace Model.Characters
{
    public sealed class CharacterFactory
    {
        public Character CreatePlayer(Vector2Int spawnPosition, CharacterControllInputReceiver receiver, ReactiveProperty<bool> canIgnoreWall, IWorld world)
        {
            return new Character(spawnPosition, new PlayerBehavior(receiver), canIgnoreWall, world);
        }
        public Character CreateCharacter(EnemyData data, Vector2Int spawnPosition, ICharacterBehavior behavior,
            ReactiveProperty<bool> canIgnoreWall, IWorld world)
        {
            return new Character(data, spawnPosition, behavior, canIgnoreWall, world);
        }
    }
}