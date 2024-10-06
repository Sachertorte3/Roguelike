using Domain.Model.Character;

namespace Domain.Model.Condition
{
    public interface IHasCondition
    {
        public string GetName(IHasAffiliation player);
        public IAffiliation Affiliation { get; }
        public IStatusManager StatusManager { get; }
    }
}