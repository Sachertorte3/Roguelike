#nullable enable
using Data.Character;
using Data.Effect;
using Model.Domain.Characters.Behavior;
using R3;
using UnityEngine;

namespace Model.Domain.Characters
{
    public sealed class CharacterFactory
    {
        public Character CreatePlayer(Vector2Int spawnPosition, CharacterControllInputReceiver receiver,
            ReactiveProperty<bool> canIgnoreWall, IMap world)
        {
            return new Character(spawnPosition, new PlayerBehavior(receiver), canIgnoreWall, world,
                CharacterGroup.Player);
        }

        public Character CreateCharacter(EnemyData data, Vector2Int spawnPosition, ICharacterBehavior behavior,
            ReactiveProperty<bool> canIgnoreWall, IMap world)
        {
            return new Character(data, spawnPosition, behavior, canIgnoreWall, world, CharacterGroup.Enemy);
        }
    }
}