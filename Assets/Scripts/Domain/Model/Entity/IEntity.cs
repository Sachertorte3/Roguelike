using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Map;
using Utilities;

namespace Domain.Model.Entity
{
    public interface IEntity : IDisposable
    {
        public EntityBase Entity { get; }
        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map);
    }
}