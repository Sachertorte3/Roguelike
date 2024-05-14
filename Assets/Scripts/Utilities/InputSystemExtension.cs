using R3;
using UnityEngine.InputSystem;

namespace Utilities
{
    public static class UnityInputSystemExtensions

    {
        public static Observable<InputAction.CallbackContext> AsObservable(this InputAction action)
        {
            return Observable.FromEvent<InputAction.CallbackContext>(
                h => action.performed += h,
                h => action.performed -= h);
        }

        public static ReadOnlyReactiveProperty<T> AsReactiveProperty<T>(this InputAction action) where T : struct
        {
            return Observable.FromEvent<InputAction.CallbackContext>(
                h =>
                {
                    action.performed += h;
                    action.canceled += h;
                },
                h =>
                {
                    action.performed -= h;
                    action.canceled += h;
                }).Select(context => context.ReadValue<T>()).ToReadOnlyReactiveProperty();
        }
    }
}