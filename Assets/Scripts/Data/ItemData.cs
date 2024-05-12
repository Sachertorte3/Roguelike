#nullable enable
using System;
using UnityEngine;

namespace Scripts.Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Item")]
    public class ItemData : ScriptableObject
    {
        public Sprite Icon;
        public SkillData Skill;
        public int UsageLimit;
    }
}
