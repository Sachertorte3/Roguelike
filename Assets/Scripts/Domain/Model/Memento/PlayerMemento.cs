#nullable enable
using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class PlayerMemento
    {
        [field: SerializeField] public CharacterMemento Character { get; private set; }
        [field: SerializeField] public int Money { get; private set; }
        public PlayerMemento(CharacterMemento character, int money)
        {
            Character = character;
            Money = money;
        }
        public PlayerMemento CopyWith(CharacterMemento? character = null, int? money = null)
        {
            return new PlayerMemento(character ?? Character, money ?? Money);
        }
    }
}