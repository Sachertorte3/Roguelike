using Scripts.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using UniRx;

namespace Assets.Scripts.View
{
    public class MenuController: MonoBehaviour
    {
        [SerializeField] GameObject _menu;
        [Inject]
        public void Construct(InputReceiver inputReceiver)
        {
            inputReceiver.OnMenuOpening.Subscribe(_ => _menu.SetActive(true));
            inputReceiver.OnMenuClosing.Subscribe(_ => _menu.SetActive(false));
        }
    }
}
