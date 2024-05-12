using R3;
using UnityEngine;
using VContainer;

namespace Scripts.View.UI
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private GameObject _menu;
        [Inject]
        public void Construct(InputReceiver inputReceiver)
        {
            inputReceiver.OnMenuOpening.Subscribe(_ => _menu.SetActive(true));
            inputReceiver.OnMenuClosing.Subscribe(_ => _menu.SetActive(false));
        }
    }
}
