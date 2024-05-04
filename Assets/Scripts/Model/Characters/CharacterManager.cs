#nullable enable
using ObservableCollections;
using R3;
using Scripts.Model.Characters.Behavior;
using Scripts.Model.Setting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Scripts.Model.Characters
{
    public sealed class CharacterManager
    {
        public Character? Player => _player;
        private Character? _player = null;
        private ObservableList<Character> _characters = new ObservableList<Character>();
        public ReadOnlyCollection<Character> Characters => new ReadOnlyCollection<Character>(_characters);
        public Observable<Character> OnCharacterAdded => _characters.ObserveAdd().Select(character => character.Value);
        private readonly CharacterFactory _factory = new CharacterFactory();
        public CharacterManager()
        {
            _characters.ObserveAdd().Subscribe(character =>
            {
                character.Value.Position.Subscribe(_ => SetAllCharacterPosition());
            });
        }
        private void AddCharacter(Character character)
        {
            _characters.Add(character);
            character.OnDead.Subscribe(_ => _characters.Remove(character));
        }
        public void SpawnPlayer(Vector2Int spawnPosition, ActionReceiver actionReceiver)
        {
            _player = _factory.CreateCharacter(spawnPosition, new PlayerBehavior(actionReceiver), Settings.IgnoreWall);
            AddCharacter(_player);
        }
        public void SpawnCharacter(Vector2Int spawnPosition)
        {
            AddCharacter(_factory.CreateCharacter(spawnPosition, new EnemyBehavior(), new ReactiveProperty<bool>(false)));
        }
        public HashSet<Vector2Int> GetAllCharacterPositions() => _allCharacterPositions;
        private HashSet<Vector2Int> _allCharacterPositions = new HashSet<Vector2Int>();
        private void SetAllCharacterPosition()
        {
            _allCharacterPositions = Characters.Select(character => character.Position.CurrentValue).ToHashSet();
        }
    }
}
