using System;

namespace Utilities.ObjectsManager
{
    public interface IDestroyObservable
    {
        event Action OnDestroy;
    }
}