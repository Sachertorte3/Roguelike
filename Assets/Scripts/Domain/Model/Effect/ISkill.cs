namespace Domain.Model.Effect
{
    public interface ISkill
    {
        public bool IsDirectional { get; }
        public bool IsUsable();
        public float EvaluatePrice();
    }
}