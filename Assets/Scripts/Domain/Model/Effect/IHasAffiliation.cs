namespace Domain.Model.Effect
{
    public interface IHasAffiliation
    {
        public IAffiliation Affiliation { get; }

        public bool IsAlly(IHasAffiliation target)
        {
            return Affiliation.IsAlly(target.Affiliation);
        }

        public bool IsEnemy(IHasAffiliation target)
        {
            return Affiliation.IsEnemy(target.Affiliation);
        }

        public bool IsNeutral(IHasAffiliation target)
        {
            return !IsAlly(target) && !IsEnemy(target);
        }
    }
}