using System;
using UniRx;
using UnityEngine.InputSystem;

public static class UnityInputSystemExtensions

{
    public static IObservable<InputAction.CallbackContext> AsObservable(this InputAction action)
    {
        return Observable.FromEvent<InputAction.CallbackContext>(
            h => action.performed += h,
            h => action.performed -= h);
    }
    public static ReadOnlyReactiveProperty<T> AsReactiveProperty<T>(this InputAction action) where T: struct
    {
        return Observable.FromEvent<InputAction.CallbackContext>(
            h => action.performed += h,
            h => action.performed -= h).Select(context => context.ReadValue<T>()).ToReadOnlyReactiveProperty();
    }
}