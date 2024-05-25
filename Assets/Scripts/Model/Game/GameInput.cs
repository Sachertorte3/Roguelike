using System;
using Model.Domain;
using VContainer;

namespace Model.Game
{
    public class GameInput : IInput
    {
        private bool _isDash;
        private bool _isNoMove;

        public void SetDash(bool isDash)
        {
            _isDash = isDash;
        }
        public void SetNoMove(bool isNoMove)
        {
            _isNoMove = isNoMove;
        }

        public bool IsDash() => _isDash;
        public bool IsNoMove() => _isNoMove;
    }
}