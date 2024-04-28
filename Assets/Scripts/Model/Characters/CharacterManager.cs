#nullable enable
using Scripts.Model.Characters.Behavior;
using System.Collections.ObjectModel;
using UniRx;

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
        public void SpawnPlayer(ActionReceiver actionReceiver)
        {
            _player = _factory.CreateCharacter(new PlayerBehavior(actionReceiver));
            _characters.Add(_player);
        }
        public void SpawnCharacter()
        {
            _characters.Add(_factory.CreateCharacter(new EnemyBehavior()));
        }
    }
    internal sealed class CharacterFactory
    {
        public Character CreateCharacter(ICharacterBehavior behavior)
        {
            return new Character(behavior);
        }
    }
}
