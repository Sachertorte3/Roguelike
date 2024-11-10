using Domain.Model.Character;
using Domain.Model.Character.Status;

namespace Domain.Service.Effect
{
    public interface ITarget
    {
        public IStats Stats { get; }
    }
}