using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Unity.Logging;

namespace View
{
    internal class LifeTimer : MonoBehaviour
    {
        public int LifeTimeMilliseconds = 1000;

        private async void Start()
        {
            try
            {
                await UniTask.Delay(LifeTimeMilliseconds, cancellationToken: destroyCancellationToken);
                Destroy(gameObject);
            }
            catch (OperationCanceledException e)
            {
                Log.Debug($"OperationCanceledException: {e}");
            }
        }
    }
}