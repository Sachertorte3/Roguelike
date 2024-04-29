#nullable enable
using Scripts.Model.Characters.Behavior;
using UniRx;
using UnityEngine;

namespace Scripts.Model.Characters
{
    public sealed class CharacterManager
    {
        public Character? Player;
        private Character? _player = null;
        private ReactiveCollection<Character> _characters = new ReactiveCollection<Character>();
        public IReadOnlyReactiveCollection<Character> Characters => _characters;
        private readonly CharacterFactory _factory = new CharacterFactory();
        public CharacterManager()
        {
        }
        public void SpawnPlayer(Vector2Int spawnPosition, ActionReceiver actionReceiver)
        {
            _player = _factory.CreateCharacter(spawnPosition, new PlayerBehavior(actionReceiver));
            _characters.Add(_player);
        }
        public void SpawnCharacter(Vector2Int spawnPosition)
        {
            _characters.Add(_factory.CreateCharacter(spawnPosition, new EnemyBehavior()));
        }
    }
    internal sealed class CharacterFactory
    {
        public Character CreateCharacter(Vector2Int spawnPosition, ICharacterBehavior behavior)
        {
            return new Character(spawnPosition, behavior);
        }
    }
}
