using Domain.Model.Character;

namespace Domain.Service.Effect
{
    public interface ITarget
    {
        public IStats Stats { get; }
    }
}