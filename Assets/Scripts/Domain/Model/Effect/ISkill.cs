namespace Domain.Model.Effect
{
    public interface ISkill : IHasUpgrades
    {
        public bool IsDirectional { get; }
        public float EvaluatePrice();
    }
}