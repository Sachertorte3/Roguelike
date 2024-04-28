using System.Collections.ObjectModel;
using UniRx;

namespace Scripts.Model
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
        public Character SpawnPlayer()
        {
            _player = _factory.CreateCharacter();
            _characters.Add(_player);
            return _player;
        }
        public void SpawnCharacter()
        {
            _characters.Add(_factory.CreateCharacter());
        }
    }
    internal sealed class CharacterFactory
    {
        public Character CreateCharacter()
        {
            return new Character();
        }
    }
}
