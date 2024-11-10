using Domain.Model.Character;
using Domain.Model.Character.Status;

namespace Domain.Model.Condition
{
    public interface IHasCondition
    {
        public string GetName(IPlayer player);
        public IAffiliation Affiliation { get; }
        public IStatusManager Status { get; }
    }
}