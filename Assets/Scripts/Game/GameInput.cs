using Domain.Model;

namespace Game
{
    public class GameInput : IInput
    {
        private bool _isDash;
        private bool _isNoMove;
        private bool _isDiagonalOnly;

        public bool IsDash()
        {
            return _isDash;
        }

        public bool IsNoMove()
        {
            return _isNoMove;
        }

        public bool IsDiagonalOnly()
        {
            return _isDiagonalOnly;
        }

        public void SetDash(bool isDash)
        {
            _isDash = isDash;
        }

        public void SetNoMove(bool isNoMove)
        {
            _isNoMove = isNoMove;
        }

        public void SetDiagonalOnly(bool isDiagonalOnly)
        {
            _isDiagonalOnly = isDiagonalOnly;
        }
    }
}