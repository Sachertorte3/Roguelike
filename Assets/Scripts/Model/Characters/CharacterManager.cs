#nullable enable
using Assets.Scripts.Model.Items;
using ObservableCollections;
using R3;
using Scripts.Data.Area;
using Scripts.Model.Characters.Behavior;
using Scripts.Model.Characters.Effect;
using Scripts.Model.Entities;
using Scripts.Model.Map;
using Scripts.Model.Setting;
using Scripts.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using VContainer;

namespace Scripts.Model.Characters
{
    public sealed class CharacterManager
    {
        public Character Player => _player;
        private Character _player;
        private ObservableList<Character> _characters = new ObservableList<Character>();
        public ReadOnlyCollection<Character> Characters => new ReadOnlyCollection<Character>(_characters);
        public Observable<Character> OnCharacterAdded => _characters.ObserveAdd().Select(character => character.Value);
        public Observable<Character> OnCharacterRemoved => _characters.ObserveRemove().Select(character => character.Value);
        private readonly CharacterFactory _factory = new();
        public readonly CharacterEvents PlayerEvents = new();
        public readonly CharacterEvents CharacterEvents = new();
        [Inject]
        public CharacterManager(Tilemap tilemap, CharacterControllInputReceiver actionReceiver)
        {
            CharacterEvents.OnPositionChanged.Subscribe(_ => SetAllCharacterPosition());
            CharacterEvents.OnDead.Subscribe(dead => _characters.Remove(dead.Character));

            _player = _factory.CreateCharacter(tilemap.GetAllPassablePositions().GetAtRandom(), new PlayerBehavior(actionReceiver), Settings.IgnoreWall);
            AddCharacter(_player);
            PlayerEvents.Add(_player);
            PlayerEvents.OnVisibleAreaChanged.Subscribe(areaChanged =>
            {
                foreach (var character in _characters)
                {
                    if (areaChanged.AreaExited.Contains(character.CurrentPosition))
                    {
                        character.VisibleByPlayer = false;
                    }
                    else if (areaChanged.AreaEntered.Contains(character.CurrentPosition))
                    {
                        character.VisibleByPlayer = true;
                    }
                }
            });
        }
        private void AddCharacter(Character character)
        {
            _characters.Add(character);
            CharacterEvents.Add(character);
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
    public class CharacterEvents
    {
        public Observable<OnPositionChangedMessage> OnPositionChanged => _onPositionChanged;
        private readonly Subject<OnPositionChangedMessage> _onPositionChanged = new();
        public Observable<OnDirectionChangedMessage> OnDirectionChanged => _onDirectionChanged;
        private readonly Subject<OnDirectionChangedMessage> _onDirectionChanged = new();
        public Observable<OnDeadMessage> OnDead => _onDead;
        private readonly Subject<OnDeadMessage> _onDead = new();
        public Observable<OnMoveMessage> OnMove => _onMove;
        private readonly Subject<OnMoveMessage> _onMove = new();
        public Observable<OnUseSkillMessage> OnUseSkill => _onUseSkill;
        private readonly Subject<OnUseSkillMessage> _onUseSkill = new();
        public Observable<OnVisibleAreaChangedMessage> OnVisibleAreaChanged => _onVisibleAreaChanged;
        private readonly Subject<OnVisibleAreaChangedMessage> _onVisibleAreaChanged = new();
        public void Add(Character character)
        {
            character.Position.Subscribe(positionChanged => _onPositionChanged.OnNext(new OnPositionChangedMessage(character, positionChanged)));
            character.Direction.Subscribe(directionChanged => _onDirectionChanged.OnNext(new OnDirectionChangedMessage(character, directionChanged)));
            character.OnDead.Subscribe(_ => _onDead.OnNext(new OnDeadMessage(character)));
            character.OnMove.Subscribe(move => _onMove.OnNext(new OnMoveMessage(character, move.direction, move.destination)));
            character.OnUseSkill.Subscribe(useSkill => _onUseSkill.OnNext(new OnUseSkillMessage(character, useSkill.skill, useSkill.position, useSkill.direction)));
            character.Area.OnVisibleAreaChanged.Pairwise().Subscribe(visibleAreaChanged =>
            {
                HashSet<Vector2Int> newArea = new HashSet<Vector2Int>(visibleAreaChanged.Current);
                visibleAreaChanged.Previous.ExceptWith(visibleAreaChanged.Current);
                visibleAreaChanged.Current.ExceptWith(visibleAreaChanged.Previous);
                _onVisibleAreaChanged.OnNext(new OnVisibleAreaChangedMessage(character, newArea, visibleAreaChanged.Previous, visibleAreaChanged.Current));
            });
        }
    }
    public record OnPositionChangedMessage(Character Character, Vector2Int Position);
    public record OnDirectionChangedMessage(Character Character, Direction8 Direction);
    public record OnDeadMessage(Character Character);
    public record OnMoveMessage(Character Character, Direction8 Direction, Vector2Int Destination);
    public record OnUseSkillMessage(Character Character, Skill Skill, Vector2Int Position, Direction8 Direction);
    public record OnVisibleAreaChangedMessage(Character Character, HashSet<Vector2Int> NewArea, HashSet<Vector2Int> AreaExited, HashSet<Vector2Int> AreaEntered);
}
