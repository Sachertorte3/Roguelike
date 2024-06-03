using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace View
{
    public class DamageTextSpawner : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private Canvas canvas;

        public void ShowDamage(Vector2Int position, int value, float percentageFromMaxHP) =>
            SpawnText(position, value, percentageFromMaxHP, Color.red);

        public void ShowHeal(Vector2Int position, int value, float percentageFromMaxHP) =>
            SpawnText(position, value, percentageFromMaxHP, Color.green);

        public void SpawnText(Vector2Int position, int value, float percentageFromMaxHP, Color color)
        {
            TextMeshProUGUI text = Instantiate(damageText, canvas.transform);
            text.text = value.ToString();
            text.transform.position = position + new Vector2(0, 0.5f);
            if (percentageFromMaxHP > 1)
            {
                text.fontSize = 0.6f;
            }
            else if (percentageFromMaxHP > 0.25)
            {
                text.fontSize = 0.5f;
            }
            else if (percentageFromMaxHP > 0.1)
            {
                text.fontSize = 0.4f;
            }
            else
            {
                text.fontSize = 0.3f;
            }

            text.color = color;
        }
    }
}