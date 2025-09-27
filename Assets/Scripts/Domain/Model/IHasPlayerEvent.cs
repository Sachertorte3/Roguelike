using System.Collections.Generic;
using R3;

namespace Domain.Model
{
    public interface IHasPlayerEvent
    {
        public IReadOnlyList<IPlayerEvent> Events { get; }
    }
}