#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data.Character;
using Data.Character.Type;
using Data.Effect;
using Data.Setting;
using Model.Domain.Action;
using Model.Domain.Characters.Behavior;
using Model.Domain.Effect;
using Model.Domain.Entities;
using Model.Domain.Items;
using Model.Domain.Logs;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Model.Domain.Characters
{
    public sealed class Character : IDisposable, ISerializable<CharacterMemento>, IEntity, IActor, IHasBehavior, IActorOfEffect
    {
        private readonly CharacterAffiliationManager _affiliationManager;
        private readonly VisionRange _area;
        private readonly ReactiveProperty<Direction8> _direction = new(Direction8.Down);
        private readonly Entity _entity;
        private readonly Inventory _inventory;
        private string _name = "Character";
        public string Name => _name;
        private readonly Subject<IEnumerable<Vector2Int>> _onSpawnEffect = new();
        private readonly CharacterStatusManager _statusManager;
        private bool _canAct = true;
        private bool _canIgnoreWall;
        public CharacterState State = CharacterState.Think;

        public static CharacterMemento BuildPlayer(Vector2Int spawnPosition)
        {
            return new CharacterMemento(
                "Player",
                new Human(Addressables
                    .LoadAssetAsync<Texture>("Assets/Images/Characters/Chara_Hero1_USM.png").WaitForCompletion()),
                new CharacterStatusMemento(20, 20, 1),
                new EntityMemento(spawnPosition),
                new InventoryMemento(new ItemMemento[10]),
                new AffiliationMemento(CharacterGroup.Player)
            );
        }
        public static CharacterMemento BuildCharacter(EnemyData data, Vector2Int spawnPosition)
        {
            return new CharacterMemento(
                data.Name,
                data.CharacterType,
                new CharacterStatusMemento(data.Hp, data.Hp , data.Strength),
                new EntityMemento(spawnPosition),
                new InventoryMemento(new ItemMemento[10]),
                new AffiliationMemento(CharacterGroup.Enemy)
            );
        }

        internal Character(EnemyData data, Vector2Int position, ICharacterBehavior behavior,
            Observable<bool> canIgnoreWall, IMap world, CharacterGroup group) : this
            (
                new CharacterMemento(
                    data.Name,
                    data.CharacterType,
                    new CharacterStatusMemento(data.Hp, data.Hp , data.Strength),
                    new EntityMemento(position),
                    new InventoryMemento(new ItemMemento[10]),
                    new AffiliationMemento(group)
                ),
                behavior,
                canIgnoreWall,
                world
            )
        { }

        internal Character(CharacterMemento data, ICharacterBehavior behavior, Observable<bool> canIgnoreWall, IMap world)
        {
            _name = data.Name;
            CharacterType = data.CharacterType;
            _entity = new Entity(data.EntityData);
            _inventory = new(data.Inventory);
            _statusManager = new CharacterStatusManager(data.Name, data.Status);
            Behavior = behavior;
            _area = new VisionRange(_entity.Position, world);
            canIgnoreWall.Subscribe(x => _canIgnoreWall = x);
            _affiliationManager = new CharacterAffiliationManager(data.Affiliation);
        }

        public CharacterMemento Serialize()
        {
            return new CharacterMemento(
                _name,
                CharacterType,
                _statusManager.Serialize(),
                _entity.Serialize(),
                _inventory.Serialize(),
                _affiliationManager.Serialize()
            );
        }

        public bool CanAct => _canAct;
        public ReadOnlyReactiveProperty<Direction8> Direction => _direction;
        public Observable<IEnumerable<Vector2Int>> OnSpawnEffect => _onSpawnEffect;
        public Observable<Unit> OnDead => _statusManager.OnDead;
        public ICharacterType CharacterType { get; init; }
        private ICharacterBehavior Behavior { get; }
        public IStatusManager StatusManager => _statusManager;
        public IAffiliation Affiliation => _affiliationManager;
        public Direction8 CurrentDirection => Direction.CurrentValue;
        public IInventory Inventory => _inventory;

        /// <summary>
        ///     Returns whether movement is possible in that direction. If it is possible to pass through walls, this is true even
        ///     if the destination is not passable.
        ///     If you want to check whether the destination is passable, please use World.IsPassable.
        /// </summary>
        public bool CanMove(Direction8 direction, IMap world)
        {
            return _canIgnoreWall
                ? world.IsMapPassable(Position.CurrentValue + direction.Vector())
                : world.IsPassable(Position.CurrentValue + direction.Vector())
                  && (!direction.IsDiagonal() ||
                      (world.IsPassable(Position.CurrentValue + direction.Rotate45Clockwise().Vector()) &&
                       world.IsPassable(Position.CurrentValue + direction.Rotate45AntiClockwise().Vector())));
        }

        public void Turn(Direction8 direction)
        {
            _direction.Value = direction;
        }

        public void DoNothing()
        {
            State = CharacterState.Wait;
        }

        public async UniTask Move(Direction8 direction, IInput input)
        {
            Debug.Log($"{_name}が{direction}に移動した");
            Turn(direction);
            await _entity.Move(direction,
                input.IsDash() ? Settings.DashMilliseconds.Value : Settings.MoveMilliseconds.Value);

            State = CharacterState.Wait;
        }

        public async UniTask UseSkill(Skill skill, Direction8 direction, IMap world)
        {
            _direction.Value = direction;
            _onSpawnEffect.OnNext(skill.GetArea(CurrentPosition, CurrentDirection));
            if (_entity.VisibleByPlayer.CurrentValue)
                await UniTask.WhenAll(skill.Use(this, CurrentPosition, direction, world),
                    UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
            else
                await skill.Use(this, CurrentPosition, direction, world);

            State = CharacterState.Wait;
        }

        public async UniTask UseItem(int itemIndex, Direction8 direction, IMap world)
        {
            Turn(direction);
            var item = _inventory.GetItem(itemIndex);
            if (item == null) throw new Exception("item is null");

            if (item.EffectsOnUse)
            {
                GameLog.Add($"{_name}:{item.Name}を使った");
                _onSpawnEffect.OnNext(item.Skill.GetArea(CurrentPosition, CurrentDirection));
                if (_entity.VisibleByPlayer.CurrentValue)
                    await UniTask.WhenAll(item.Use(this, CurrentPosition, direction, world),
                        UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
                else
                    await item.Use(this, CurrentPosition, direction, world);

                State = CharacterState.Wait;
            }
        }

        public async UniTask ThrowItem(int itemIndex, Direction8 direction, IMap world)
        {
            Turn(direction);
            var item = _inventory.Remove(itemIndex);
            if (item == null) throw new Exception("item is null");
            var itemEntity = world.SpawnItem(item, CurrentPosition);
            GameLog.Add($"{_name}:{item.Name}を投げた");
            if (_entity.VisibleByPlayer.CurrentValue)
                await UniTask.WhenAll(itemEntity.Throw(this, direction, world),
                    UniTask.Delay(Settings.EffectDisplayTime.CurrentValue));
            else
                await itemEntity.Throw(this, direction, world);

            State = CharacterState.Wait;
        }

        public void Dispose()
        {
            _entity.Dispose();
            _inventory.Dispose();
            _onSpawnEffect.Dispose();
            _direction.Dispose();
        }

        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;
        public Entity Entity => _entity;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;

        public void SetVisiblity(bool visiblity)
        {
            _entity.SetVisibility(visiblity);
        }

        public IVisionRange Area => _area;

        public void Teleport(Vector2Int position)
        {
            _entity.Teleport(position);

            State = CharacterState.Wait;
        }

        public void WasAttackedBy(IActorOfEffect actor, float impact)
        {
            var direction = DirectionMethods.NearestDirectionFromVector(actor.CurrentPosition - CurrentPosition);
            if (direction.HasValue)
            {
                Turn(direction.Value);
            }
            _affiliationManager.OnCharacterAttacked(actor.Affiliation, Affiliation, impact);
        }
        public void WasHealedBy(IActorOfEffect actor, float impact)
        {
            _affiliationManager.OnCharacterHealed(actor.Affiliation, Affiliation, impact);
        }

        ~Character()
        {
            Dispose();
        }

        public string TypeName()
        {
            return CharacterType.TypeName();
        }

        public string SubtypeName()
        {
            return CharacterType.SubtypeName();
        }

        public async UniTask DoNextAction(IMap world, IInput input)
        {
            State = CharacterState.Think;
            var action = await Behavior.GenerateNextAction(this, world, input);
            State = CharacterState.Act;
            await action.Do(this, world, input);
        }

        public bool TryPickUp(Item item)
        {
            return _inventory.TryAdd(item);
        }

        public Item? ReplaceInventory(Item? item, int index)
        {
            return _inventory.Replace(item, index);
        }

        public void UpdateTurn(IMap world)
        {
            _statusManager.UpdateTurn();
            _affiliationManager.UpdateTurn(world.GetVisibleCharacters(this).Select(x => x.Affiliation));
        }
    }
    public static class CharacterExtensions
    {
        public static bool IsVisible(this Character character, Vector2Int position)
        {
            return character.Area.VisibleArea.Contains(position);
        }
        public static bool IsAlly(this IActorOfEffect character, Character target)
        {
            return character.Affiliation.IsAlly(target.Affiliation);
        }
        public static bool IsEnemy(this IActorOfEffect character, Character target)
        {
            return character.Affiliation.IsEnemy(target.Affiliation);
        }
    }
}

