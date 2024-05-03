using Scripts.Utilities;

namespace Scripts.Model.Characters.Behavior
{
    internal interface IHasBehavior
    {
        public bool CanMove(Direction8 direction);
    }
}
