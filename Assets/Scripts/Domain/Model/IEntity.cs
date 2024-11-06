using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Map;
using Utilities;

namespace Domain.Model
{
    public interface IEntity : IDisposable
    {
        public Entity Entity { get; }
        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map);
    }
}