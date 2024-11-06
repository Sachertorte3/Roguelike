using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Model.Memento;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Fire : ISerializable<EntityMemento>, IEntity
    {
        public Entity Entity { get; init; }

        public Fire(EntityMemento memento)
        {
            Entity = new Entity(memento);
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map) => UniTask.CompletedTask;
        public void Dispose() => Entity.Dispose();
        public EntityMemento Serialize() => Entity.Serialize();
        public static EntityMemento Build(Vector2Int position)
        {
            return Entity.Build(position, EntityLayer.Top);
        }
    }
}