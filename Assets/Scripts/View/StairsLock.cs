using TMPro;
using UnityEngine;

namespace UI
{
    public class StairsLock : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private MeshRenderer _meshRenderer;
        [SerializeField] private TMP_Text countText;
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _meshRenderer = countText.GetComponent<MeshRenderer>();
            _meshRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
        }
        public void SetVisibility(bool visibility)
        {
            _spriteRenderer.enabled = visibility;
            _meshRenderer.enabled = visibility;
        }
        public void SetCount(int count)
        {
            countText.text = count.ToString();
        }
        public void UnLock()
        {
            Destroy(gameObject);
        }
    }
}