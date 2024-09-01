namespace Domain.Model.Effect
{
    public interface ISkill : IHasUpgrades
    {
        public float EvaluatePrice();
    }
}