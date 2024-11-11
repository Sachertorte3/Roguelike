using TMPro;
using UnityEngine;

namespace View
{
    public class DamageTextSpawner : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private Canvas canvas;

        public void ShowDamage(Vector2Int position, int value, int percentageFromMaxHP, int textDisplayMilliseconds)
        {
            SpawnText(position, value, percentageFromMaxHP, textDisplayMilliseconds, Color.red);
        }

        public void ShowHeal(Vector2Int position, int value, int percentageFromMaxHP, int textDisplayMilliseconds)
        {
            SpawnText(position, value, percentageFromMaxHP, textDisplayMilliseconds, Color.green);
        }

        public void SpawnText(Vector2Int position, int value, int percentageFromMaxHP, int textDisplayMilliseconds,
            Color color)
        {
            damageText.GetComponent<LifeTimer>().LifeTimeMilliseconds = textDisplayMilliseconds;
            var text = Instantiate(damageText, canvas.transform);
            text.text = value.ToString();
            var random = new Vector2(Random.value - 0.5f, Random.value - 0.5f);
            text.transform.position = position + new Vector2(0, 0.5f) + random;
            if (percentageFromMaxHP > 100)
            {
                text.fontSize = 1f;
            }
            else if (percentageFromMaxHP > 25)
            {
                text.fontSize = 0.8f;
            }
            else if (percentageFromMaxHP > 10)
            {
                text.fontSize = 0.7f;
            }
            else
            {
                text.fontSize = 0.5f;
            }

            text.color = color;
        }

        public void DeleteAllText()
        {
            foreach (var text in canvas.GetComponentsInChildren<DamageText>())
            {
                Destroy(text.gameObject);
            }
        }
    }
}