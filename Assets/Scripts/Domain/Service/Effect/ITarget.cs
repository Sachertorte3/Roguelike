using Domain.Model.Characters;

namespace Domain.Service.Effect
{
    public interface ITarget
    {
        public IStats Stats { get; }
    }
}