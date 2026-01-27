using Domain.Model;
using R3;

namespace Game
{
    public class GameInput : IInput
    {
        private ReadOnlyReactiveProperty<bool> _isDash;
        private ReadOnlyReactiveProperty<bool> _isNoMove;
        private ReadOnlyReactiveProperty<bool> _isDiagonalOnly;

        public void Bind(ReadOnlyReactiveProperty<bool> isDash, ReadOnlyReactiveProperty<bool> isNoMove, ReadOnlyReactiveProperty<bool> isDiagonalOnly)
        {
            _isDash = isDash;
            _isNoMove = isNoMove;
            _isDiagonalOnly = isDiagonalOnly;
        }


        public bool IsDash() => _isDash.CurrentValue    ;

        public bool IsNoMove() => _isNoMove.CurrentValue;

        public bool IsDiagonalOnly() => _isDiagonalOnly.CurrentValue;
    }
}