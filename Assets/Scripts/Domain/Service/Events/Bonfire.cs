using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Bonfire : ISerializable<EntityMemento>, IEventEntity
    {
        public EntityBase Entity { get; init; }

        public Bonfire(EntityMemento memento)
        {
            Entity = new EntityBase(memento);
            Event = new CharacterEvent(
                character => false,
                (character, gameManager, map) => UniTask.FromResult(false)
            );
        }

        public ICharacterEvent Event { get; init; }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public EntityMemento Serialize()
        {
            return Entity.Serialize();
        }

        public static EntityMemento Build(Vector2Int position)
        {
            return EntityBase.Build(position, EntityLayer.Middle);
        }
    }
}