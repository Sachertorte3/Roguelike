#nullable enable
using System;
using Domain.Model.Character;
using UnityEngine;
using Utilities.Serialize;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ChestMemento
    {
        [field: SerializeField] public Option<IItemMemento> Item { get; private set; }
        [SerializeField] private Option<ScriptableObjectSerializable<EnemyData>> _mimic;
        public Option<EnemyData> Mimic => _mimic.Map(m => m.Value);
        [field: SerializeField] public EntityMemento Entity { get; private set; }

        public ChestMemento(
            Option<IItemMemento> item,
            Option<EnemyData> mimic,
            EntityMemento entity)
        {
            Item = item;
            _mimic = mimic.Map(m => m.ToSerializable());
            Entity = entity;
        }

        public ChestMemento(IItemMemento item, EntityMemento entity) : this(
            Option.Some(item),
            Option.None<EnemyData>(),
            entity)
        {
        }

        public ChestMemento(EnemyData mimic, EntityMemento entity) : this(
            Option.None<IItemMemento>(),
            Option.Some(mimic),
            entity)
        {
        }
    }
}