using Domain.Model.Character;

namespace Domain.Model.Condition
{
    public interface IHasCondition : IHasStatus, IHasAffiliation
    {
        public string GetName(IPlayer player);
    }
}