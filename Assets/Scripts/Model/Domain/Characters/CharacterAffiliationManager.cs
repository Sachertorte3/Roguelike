using System.Collections.Generic;
using System.Diagnostics;
using Data;
using Data.Character;
using Data.Effect;
using Unity.Logging;

namespace Model.Domain.Characters
{
    public class CharacterAffiliationManager : IAffiliation, ISerializable<AffiliationMemento>
    {
        private const int AffectionAllyThreshold = 10; // 味方と見なす好感度の閾値
        private const int AffectionEnemyThreshold = -10; // 敵と見なす好感度の閾値
        private const int BaseAllyValue = 20; // 同じグループの基本好感度
        private const int BaseEnemyValue = -20; // 敵対グループの基本好感度

        public CharacterAffiliationManager(AffiliationMemento data)
        {
            Group = data.Group;
        }

        public AffiliationMemento Serialize()
        {
            return new AffiliationMemento(Group);
        }

        public CharacterGroup Group { get; private set; }

        public bool IsAlly(IAffiliation other)
        {
            int totalAffection = GetAffectionByGroup(other) + GetAffection(other);

            return totalAffection > AffectionAllyThreshold;
        }

        public bool IsEnemy(IAffiliation other)
        {
            int totalAffection = GetAffectionByGroup(other) + GetAffection(other);

            return totalAffection < AffectionEnemyThreshold;
        }

        private Dictionary<IAffiliation, int> affections = new();

        public void ModifyAffection(IAffiliation target, int change)
        {
            if (!affections.ContainsKey(target))
            {
                affections[target] = 0;
            }

            affections[target] += change;
            Log.Debug($"affection: {affections[target]}");
        }

        public int GetAffectionByGroup(IAffiliation target)
        {
            return (Group, target.Group) switch
            {
                (CharacterGroup.Player, CharacterGroup.Player) => BaseAllyValue,
                (CharacterGroup.Player, CharacterGroup.Enemy) => BaseEnemyValue,
                (CharacterGroup.Enemy, CharacterGroup.Player) => BaseEnemyValue,
                (CharacterGroup.Enemy, CharacterGroup.Enemy) => BaseAllyValue,
                (CharacterGroup.Neutral, _) => 0,
                _ => 0,
            };
        }

        public int GetAffection(IAffiliation target)
        {
            if (affections.ContainsKey(target))
            {
                return affections[target];
            }
            return 0; // デフォルトの好感度は0とする
        }

        public void OnCharacterAttacked(IAffiliation attacker, IAffiliation target)
        {
            if (target == this)
            {
                ModifyAffection(attacker, -10); // 攻撃されると好感度が10ポイント減少
            }
            else if (attacker == this)
            {
                return;
            }
            else
            {
                int affectionToTarget = GetAffection(target);
                if (affectionToTarget > 50) // 好感度が高い場合
                {
                    ModifyAffection(attacker, -5); // 第三者の好感度が高い場合、攻撃者に対する好感度を減少
                }
                else if (affectionToTarget < -50) // 好感度が低い場合
                {
                    ModifyAffection(attacker, 5); // 第三者の好感度が低い場合、攻撃者に対する好感度を増加
                }
            }
        }
        public void OnCharacterHealed(IAffiliation healer, IAffiliation target)
        {
            if (target == this)
            {
                ModifyAffection(healer, 10); // 回復されると好感度が10ポイント増加
            }
            else if (healer == this)
            {
                return;
            }
            else
            {
                int affectionToTarget = GetAffection(target);
                if (affectionToTarget > 50) // 好感度が高い場合
                {
                    ModifyAffection(healer, 5); // 第三者の好感度が高い場合、回復者に対する好感度を増加
                }
                else if (affectionToTarget < -50) // 好感度が低い場合
                {
                    ModifyAffection(healer, -5); // 第三者の好感度が低い場合、回復者に対する好感度を減少
                }
            }
        }
    }
}