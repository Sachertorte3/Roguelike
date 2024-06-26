using Domain.Service.Characters.Stats;

namespace Domain.Service.Effect
{
    public interface ITarget
    {
        public IStats Stats { get; }
    }
}