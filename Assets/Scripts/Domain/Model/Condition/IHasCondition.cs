using Domain.Model.Character;

namespace Domain.Model.Condition
{
    public interface IHasCondition
    {
        public IAffiliation Affiliation { get; }
        public IStatusManager StatusManager { get; }
    }
}