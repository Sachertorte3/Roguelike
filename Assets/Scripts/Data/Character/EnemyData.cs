using System.Collections;
using System.Collections.Generic;
using Data.Character.Type;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/EnemyData")]
public class EnemyData : ScriptableObject
{
    [SerializeReference] public ICharacterType CharacterType;
    [MinValue(1)] public int Hp;
    [MinValue(1)] public int Strength;
}
