#nullable enable
using System;
using Domain.Model.Character;
using Domain.Model.Character.Type;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class CharacterMemento
    {
        public string Name;
        [SerializeReference] public ICharacterType CharacterType;
        public BehaviorData Behavior;
        public Option<Vector2Int> HomePosition;
        public CharacterStatusMemento Status;
        public EntityMemento Entity;
        public Direction8 Direction;
        public CharacterSkillMemento[] Skills;
        public Option<SpawnEffectSkillMemento> LastSkill;
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