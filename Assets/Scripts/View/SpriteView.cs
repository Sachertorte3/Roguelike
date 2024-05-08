using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.View
{
    public class SpriteView: MonoBehaviour
    {
        private bool _isVisible = false;
        public Vector3 Position() => transform.position;
        public bool GetVisibility() => _isVisible;
        public void SetVisibility(bool visible)
        {
            _isVisible = visible;
            GetComponent<SpriteRenderer>().enabled = visible;
        }
    }
}
