using Model.Characters.Stats;

namespace Model.Characters.Effect
{
    public interface ITarget
    {
        public IStats Stats { get; }
    }
}