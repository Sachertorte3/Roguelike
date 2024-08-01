#nullable enable
using System;
using Domain.Model.Character.Type;
using UnityEngine;
using Utilities;

namespace Domain.Model.Character
{
    [Serializable]
    public class CharacterMemento
    {
        public string Name;
        [SerializeReference] public ICharacterType CharacterType;
        public BehaviorData Behavior;
        public CharacterStatusMemento Status;
        public EntityMemento Entity;
        public Direction8 Direction;
        public CharacterSkillMemento[] Skills;
        public NullableSerializable<SkillMemento> LastSkill;
        public InventoryMemento Inventory;
        public AffiliationMemento Affiliation;
        public Aggression Aggression;
        public int Money;
        public bool IsLeader;
        public bool IsShiny;
        public bool IsBoss;
        public bool CanPickUp;
        public bool CanUseItem;
    }
}