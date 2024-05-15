using Model.Characters.Stats;

namespace Model.Effect
{
    public interface ITarget
    {
        public IStats Stats { get; }
    }
}