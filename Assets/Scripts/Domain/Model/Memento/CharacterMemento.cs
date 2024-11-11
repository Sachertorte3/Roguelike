#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Character.Type;
using UnityEngine;
using Utilities;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    [Serializable]
    public class CharacterMemento
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeReference] public ICharacterType CharacterType { get; private set; }
        [field: SerializeField] public BehaviorMemento Behavior { get; private set; }
        [field: SerializeField] public CharacterStatusMemento Status { get; private set; }
        [field: SerializeField] public EntityMemento Entity { get; private set; }
        [field: SerializeField] public Direction8 Direction { get; private set; }
        [field: SerializeField] public List<CharacterSkillMemento> Skills { get; private set; }
        [field: SerializeField] public Option<SpawnEffectSkillMemento> LastSkill { get; private set; }
        [field: SerializeField] public InventoryMemento Inventory { get; private set; }
        [field: SerializeField] public List<string> KnownItemNames { get; private set; }
        [field: SerializeField] public AffiliationMemento Affiliation { get; private set; }
        [field: SerializeField] public Aggression Aggression { get; private set; }
        [field: SerializeField] public int Money { get; private set; }
        [field: SerializeField] public bool IsLeader { get; private set; }
        [field: SerializeField] public bool IsShiny { get; private set; }
        [field: SerializeField] public bool IsBoss { get; private set; }
        [field: SerializeField] public bool IsFlying { get; private set; }
        [field: SerializeField] public bool CanThroughWalls { get; private set; }
        [field: SerializeField] public bool CanPickUp { get; private set; }
        [field: SerializeField] public bool CanUseItem { get; private set; }

        public CharacterMemento(
            string name,
            ICharacterType characterType,
            BehaviorMemento behavior,
            CharacterStatusMemento status,
            EntityMemento entity,
            Direction8 direction,
            List<CharacterSkillMemento> skills,
            Option<SpawnEffectSkillMemento> lastSkill,
            InventoryMemento inventory,
            List<string> knownItemNames,
            AffiliationMemento affiliation,
            Aggression aggression,
            int money,
            bool isLeader,
            bool isShiny,
            bool isBoss,
            bool isFlying,
            bool canThroughWalls,
            bool canPickUp,
            bool canUseItem
        )
        {
            Name = name;
            CharacterType = characterType;
            Behavior = behavior;
            Status = status;
            Entity = entity;
            Direction = direction;
            Skills = skills;
            LastSkill = lastSkill;
            Inventory = inventory;
            KnownItemNames = knownItemNames;
            Affiliation = affiliation;
            Aggression = aggression;
            Money = money;
            IsLeader = isLeader;
            IsShiny = isShiny;
            IsBoss = isBoss;
            IsFlying = isFlying;
            CanThroughWalls = canThroughWalls;
            CanPickUp = canPickUp;
            CanUseItem = canUseItem;
        }

        public CharacterMemento CopyWith(
            string? name = null,
            ICharacterType? characterType = null,
            BehaviorMemento? behavior = null,
            CharacterStatusMemento? status = null,
            EntityMemento? entity = null,
            Direction8? direction = null,
            List<CharacterSkillMemento>? skills = null,
            Option<SpawnEffectSkillMemento>? lastSkill = null,
            InventoryMemento? inventory = null,
            List<string>? knownItemNames = null,
            AffiliationMemento? affiliation = null,
            Aggression? aggression = null,
            int? money = null,
            bool? isLeader = null,
            bool? isShiny = null,
            bool? isBoss = null,
            bool? isFlying = null,
            bool? canThroughWalls = null,
            bool? canPickUp = null,
            bool? canUseItem = null
        )
        {
            return new CharacterMemento(
                name ?? Name,
                characterType ?? CharacterType,
                behavior ?? Behavior,
                status ?? Status,
                entity ?? Entity,
                direction ?? Direction,
                skills ?? Skills,
                lastSkill ?? LastSkill,
                inventory ?? Inventory,
                knownItemNames ?? KnownItemNames,
                affiliation ?? Affiliation,
                aggression ?? Aggression,
                money ?? Money,
                isLeader ?? IsLeader,
                isShiny ?? IsShiny,
                isBoss ?? IsBoss,
                isFlying ?? IsFlying,
                canThroughWalls ?? CanThroughWalls,
                canPickUp ?? CanPickUp,
                canUseItem ?? CanUseItem
            );
        }

        public CharacterMemento ReplacePosition(Vector2Int position)
        {
            return CopyWith(entity: Entity.CopyWith(position: position));
        }
    }
}