#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Domain.Service.Items;
using Domain.Service.Logs;
using Domain.Service.Rooms;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;

namespace Game
{
    public class EntityManager : ISerializable<EntitiesMemento>, IDisposable
    {
        public IPlayer Player => CharacterManager?.Player;

        private CharacterManager CharacterManager { get; init; }
        private ItemManager ItemManager { get; init; }
        private EventEntityManager EventEntityManager { get; init; }
        private ThrowAnimationEntityManager ThrowAnimationEntityManager { get; init; }
        private FireEntityManager FireEntityManager { get; init; }

        public IObservableCollection<ICharacter> Characters => CharacterManager.Characters;
        public IObservableCollection<IItemEntity> Items => ItemManager.Items;
        public IObservableCollection<IEventEntity> EventEntities => EventEntityManager.EventEntities;
        public IObservableCollection<IPlayerEventEntity> PlayerEventEntities => EventEntityManager.PlayerEventEntities;
        public IObservableCollection<IScheduledEventEntity> ScheduledEventEntities => EventEntityManager.ScheduledEventEntities;
        public IObservableCollection<IEventEntity> StandaloneEventEntities => EventEntityManager.StandaloneEventEntities;
        public IObservableCollection<IPlayerEventEntity> StandalonePlayerEventEntities => EventEntityManager.StandalonePlayerEventEntities;
        public IObservableCollection<IScheduledEventEntity> StandaloneScheduledEventEntities => EventEntityManager.StandaloneScheduledEventEntities;
        public List<Stairs> Stairs => EventEntityManager.Stairs;

        public IObservableCollection<ThrowAnimationEntity> ThrowAnimationEntities =>
            ThrowAnimationEntityManager.ThrowAnimationEntities;

        public IObservableCollection<Fire> FireEntities => FireEntityManager.FireEntities;

        private readonly ObservableList<IEntity> _entities = new();
        public IObservableCollection<IEntity> Entities => _entities;

        private readonly CompositeDisposable _disposables = new();

        public EntityManager(EntitiesMemento entitiesMemento, PlayerMemento playerData, List<CharacterMemento>? partyMembers, Vector2Int playerPosition, bool resetPertyPositions, CharacterControlInputReceiver receiver, IGameManager gameManager, IMap map)
        {
            CharacterManager = new CharacterManager(playerData, receiver, gameManager, map);
            ItemManager = new ItemManager();
            EventEntityManager = new EventEntityManager(entitiesMemento.EventEntities, map.MovementEntityLocked);
            ThrowAnimationEntityManager = new ThrowAnimationEntityManager();
            FireEntityManager = new FireEntityManager(entitiesMemento.Fires);

            _entities.AddWith(Characters).AddTo(_disposables);
            _entities.AddWith(Items).AddTo(_disposables);
            _entities.AddWith(StandaloneEventEntities).AddTo(_disposables);
            _entities.AddWith(StandalonePlayerEventEntities).AddTo(_disposables);
            _entities.AddWith(StandaloneScheduledEventEntities).AddTo(_disposables);
            _entities.AddWith(ThrowAnimationEntities).AddTo(_disposables);
            _entities.AddWith(FireEntities).AddTo(_disposables);

            foreach (var character in entitiesMemento.Characters)
            {
                SpawnCharacter(character, gameManager, map);
            }

            if (partyMembers != null)
            {
                foreach (var character in partyMembers)
                {
                    if (resetPertyPositions)
                    {
                        SpawnCharacter(
                        character.ReplacePosition(
                            map.FindBlankPositionFrom(
                                playerPosition,
                                position => !AllCharacterPositionsFast().Contains(position)
                            )
                            ),
                            gameManager,
                            map
                        );
                    }
                    else
                    {
                        SpawnCharacter(character, gameManager, map);
                    }
                }
            }

            foreach (var item in entitiesMemento.Items)
            {
                SpawnItem(item);
            }
        }

        public void Dispose()
        {
            CharacterManager.Dispose();
            ItemManager.Dispose();
            EventEntities.ForEach(eventEntity => eventEntity.Dispose());
            PlayerEventEntities.ForEach(eventEntity => eventEntity.Dispose());
            ScheduledEventEntities.ForEach(eventEntity => eventEntity.Dispose());
            ThrowAnimationEntities.ForEach(throwAnimationEntity => throwAnimationEntity.Dispose());
            FireEntities.ForEach(fireEntity => fireEntity.Dispose());
            _disposables.Dispose();
        }

        public EntitiesMemento Serialize()
        {
            return new EntitiesMemento(
                Characters.Except(new[] { Player.Character }).Select(character => character.Serialize()).ToList(),
                Items.Select(item => item.Serialize()).ToList(),
                EventEntityManager.Serialize(),
                FireEntityManager.Serialize()
            );
        }

        public EntitiesMemento SerializeWithoutPartyMembers(IEnumerable<ICharacter> partyMembers)
        {
            return new EntitiesMemento(
                Characters.Except(new[] { Player.Character }).Except(partyMembers).Select(character => character.Serialize()).ToList(),
                Items.Select(item => item.Serialize()).ToList(),
                EventEntityManager.Serialize(),
                FireEntityManager.Serialize()
            );
        }

        public void SetRules(IGameManager gameManager)
        {
            foreach (var layer in Enum.GetValues(typeof(EntityLayer)).Cast<EntityLayer>())
            {
                _allEntityPositions[layer] = new Dictionary<Vector2Int, IEntity>();
            }

            Entities.SubscribeIncludingCurrentItems(
                entity =>
                {
                    if (entity.Entity.IsVisualOnly.CurrentValue)
                        return;
                    _allEntityPositions[entity.Entity.Layer].Add(entity.Entity.CurrentPosition, entity);
                },
                entity =>
                {
                    if (entity.Entity.IsVisualOnly.CurrentValue)
                        return;
                    _allEntityPositions[entity.Entity.Layer].Remove(entity.Entity.CurrentPosition);
                }
            ).AddTo(_disposables);

            Entities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.Position.Pairwise(),
                (entity, positions) =>
                {
                    if (entity.Entity.IsVisualOnly.CurrentValue)
                        return;
                    _allEntityPositions[entity.Entity.Layer].Remove(positions.Previous);
                    _allEntityPositions[entity.Entity.Layer].Add(positions.Current, entity);
                }
            ).AddTo(_disposables);

            Entities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.IsVisualOnly.SkipLatestValueOnSubscribe(),
                (entity, _) =>
                {
                    if (entity.Entity.IsVisualOnly.CurrentValue)
                        _allEntityPositions[entity.Entity.Layer].Remove(entity.Entity.CurrentPosition);
                    else
                        _allEntityPositions[entity.Entity.Layer].Add(entity.Entity.CurrentPosition, entity);
                }
            ).AddTo(_disposables);
        }
        public void UpdateTurn(IMap map)
        {
            FireEntityManager.UpdateTurn(map);

            var characters = Characters.In(FireEntities.Positions()).ToList();
            foreach (var character in characters)
            {
                character.LoseHp(1, "は火に焼かれた");
                GameLog.Add(character.Entity.IsVisible, $"{character.GetName(Player)}は火に焼かれた");
            }

            var items = Items.In(FireEntities.Positions()).ToList();
            foreach (var item in items)
            {
                item.Entity.Destroy($"は灰になった");
                GameLog.Add(item.IsVisible, $"{item.Item.GetName(Player, map.ItemPlaceholders)}は灰になった");
            }
        }

        public ICharacter SpawnCharacter(CharacterMemento character, IGameManager gameManager, IMap map)
        {
            var ally = CharacterManager.SpawnAlly(character, gameManager, map);
            EventEntityManager.Add(ally);
            return ally.Character;
        }
        public void RemoveCharacter(ICharacter character)
        {
            CharacterManager.RemoveCharacter(character);
        }
        public void SpawnItem(ItemEntityMemento item)
        {
            ItemManager.SpawnItem(item);
        }
        public IItemEntity SpawnItem(IItem item, Vector2Int position)
        {
            return ItemManager.SpawnItem(item, position);
        }
        public void AddClerk(Clerk clerk)
        {
            EventEntityManager.Add(clerk);
        }
        public void SpawnFire(Vector2Int position)
        {
            FireEntityManager.Add(new Fire(Fire.Build(position)));
        }
        public async UniTask<Vector2Int> ShowThrowAnimation(Sprite icon, Vector2Int position, Direction8 direction,
            int distance, IMap map, params EntityLayer[] canHitLayer)
        {
            var throwAnimationEntity = new ThrowAnimationEntity(position, icon);
            ThrowAnimationEntityManager.Add(throwAnimationEntity);
            var destination = await throwAnimationEntity.Throw(direction, map, distance, canHitLayer);
            throwAnimationEntity.Entity.Destroy("は演出が終わったので消えた");
            return destination;
        }
        public IItem? GetItemByIdFromWorldOrInventory(Id<IItem> id)
        {
            var itemEntity = ItemManager.Items.ById(id);
            if (itemEntity != null)
                return itemEntity.Item;
            foreach (var character in Characters)
            {
                var item = character.Inventory.AllItems.ById(id);
                if (item != null)
                    return item;
            }

            return null;
        }

        public Vector2Int? GetItemPositionByIdFromWorldOrInventory(Id<IItem> id)
        {
            var itemEntity = ItemManager.Items.ById(id);
            if (itemEntity != null)
                return itemEntity.Entity.CurrentPosition;
            foreach (var character in Characters)
            {
                var item = character.Inventory.AllItems.ById(id);
                if (item != null)
                    return character.Entity.CurrentPosition;
            }

            return null;
        }

        public List<IEventEntity> GetEventEntityAt(Vector2Int position, EntityLayer layer)
        {
            return EventEntities.At(position).On(layer).ToList();
        }

        public List<IPlayerEventEntity> GetPlayerEventEntityAt(Vector2Int position, EntityLayer layer)
        {
            return PlayerEventEntities.At(position).On(layer).ToList();
        }

        public List<IScheduledEventEntity> GetScheduledEventEntityAt(Vector2Int position, EntityLayer layer)
        {
            return ScheduledEventEntities.At(position).On(layer).ToList();
        }
        private Dictionary<EntityLayer, Dictionary<Vector2Int, IEntity>> _allEntityPositions = new();

        public IEntity? GetEntityFastAt(Vector2Int position, EntityLayer layer)
        {
            return _allEntityPositions[layer].GetValueOrDefault(position);
        }

        public IEnumerable<IEntity> GetEntitiesFastAt(Vector2Int position, IEnumerable<EntityLayer> layers)
        {
            foreach (var layer in layers)
            {
                var entity = _allEntityPositions[layer].GetValueOrDefault(position);
                if (entity != null)
                    yield return entity;
            }
        }

        public IEnumerable<IEntity> GetEntitiesFastAt(Vector2Int position)
        {
            return GetEntitiesFastAt(position, Enum.GetValues(typeof(EntityLayer)).Cast<EntityLayer>());
        }
        public HashSet<Vector2Int> AllItemPositionsFast()
        {
            return ItemManager.GetAllItemPositions();
        }

        public HashSet<Vector2Int> AllCharacterPositionsFast()
        {
            return CharacterManager.GetAllCharacterPositions();
        }
        public IItemEntity? GetItemAt(Vector2Int position)
        {
            return ItemManager.GetItemAt(position);
        }
        public bool CanPickUpAt(Vector2Int position, bool canPickUpShopItem)
        {
            return ItemManager.CanPickUpAt(position, canPickUpShopItem);
        }
        public IItemEntity PickUpAt(Vector2Int position, bool canPickUpShopItem)
        {
            return ItemManager.PickUpAt(position, canPickUpShopItem);
        }
        public IItemEntity? TryPickUpAt(Vector2Int position, bool canPickUpShopItem)
        {
            return ItemManager.TryPickUpAt(position, canPickUpShopItem);
        }
        public void AttackStatue(IEnumerable<Vector2Int> positions)
        {
            foreach (var statue in EventEntityManager.Statues.In(positions).ToList())
            {
                statue.Attacked();
            }
        }
        /// <summary>
        ///     Gets a character that follows the player when moving from one map to another.
        ///     Does not include the player themselves.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ICharacter> GetFollowingCharacters()
        {
            return Characters
                .Where(character => !character.IsPlayer)
                .Where(character => character.IsAlly(Player.Character))
                .Where(character => character.IsVisible(Player.Character.Entity.CurrentPosition));
        }
    }
}