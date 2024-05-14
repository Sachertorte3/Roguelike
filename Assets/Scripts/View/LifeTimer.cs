using Cysharp.Threading.Tasks;
using UnityEngine;

namespace View
{
    internal class LifeTimer : MonoBehaviour
    {
        public int LifeTimeMilliseconds = 1000;

        private async void Start()
        {
            await UniTask.Delay(LifeTimeMilliseconds, cancellationToken: destroyCancellationToken);
            Destroy(gameObject);
        }
    }
}