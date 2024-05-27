using Model.Domain.Characters.Stats;

namespace Model.Domain.Effect
{
    public interface ITarget
    {
        public IStats Stats { get; }
    }
}