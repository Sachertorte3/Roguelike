using UnityEngine;

namespace Domain.Model.Dungeon
{
    [CreateAssetMenu(fileName = "Placeholders", menuName = "ScriptableObject/Placeholders")]
    public class Placeholders : ScriptableObject
    {
        public CategoryPlaceholders PotionPlaceholders;
        public CategoryPlaceholders ScrollPlaceholders;
        public CategoryPlaceholders BookPlaceholders;
        public CategoryPlaceholders WandPlaceholders;
        public CategoryPlaceholders ArtifactPlaceholders;
    }
}