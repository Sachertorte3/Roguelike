using Data.Character.Type;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data.Character
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [MinValue(1)] public int Hp;
        [MinValue(1)] public int Strength;
        [SerializeReference] public ICharacterType CharacterType;
    }
}