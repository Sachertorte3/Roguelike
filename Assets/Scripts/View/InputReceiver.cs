using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class InputReceiver : MonoBehaviour
{
    MyInputAction _actions;
    public ReadOnlyReactiveProperty<Vector2> MoveDirection => _actions.Field.Move.AsReactiveProperty<Vector2>();
    private void Start()
    {
        _actions = new MyInputAction();
        _actions.Enable();
        MoveDirection.AddTo(this);
    }
}
