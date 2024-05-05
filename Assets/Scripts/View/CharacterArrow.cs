using Cysharp.Threading.Tasks.Triggers;
using R3;
using Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.View
{
    public class CharacterArrow: MonoBehaviour
    {
        public void Constract(ReactiveProperty<Direction8> characterDirection)
        {
            characterDirection.Subscribe(direction => transform.localEulerAngles = new Vector3(0, 0, direction.Angle().Value - 90));
        }
    }
}
