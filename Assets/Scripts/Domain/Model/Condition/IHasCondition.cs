using Domain.Model.Character;
using Utilities;

namespace Domain.Model.Condition
{
    public interface IHasCondition
    {
        public IAffiliation Affiliation { get; }
        public IStatusManager StatusManager { get; }
    }
}