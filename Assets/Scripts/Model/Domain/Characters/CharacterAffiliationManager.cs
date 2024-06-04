using System.Collections.Generic;
using System.Linq;
using Data.Character;
using Data.Effect;
using R3;
using UnityEngine;

namespace Model.Domain.Characters
{
    public class CharacterAffiliationManager : IAffiliation, ISerializable<AffiliationMemento>
    {
        private const float AffectionAllyThreshold = 1f; // 味方と見なす好感度の閾値
        private const float AffectionEnemyThreshold = -0.2f; // 敵と見なす好感度の閾値
        private const float BaseAllyValue = 1.2f; // 味方グループの基本好感度
        private const float BaseEnemyValue = -1; // 敵対グループの基本好感度
        public Observable<OnAffectionChangedMessage> OnAffectionChanged => _onAffectionChanged;
        private readonly Subject<OnAffectionChangedMessage> _onAffectionChanged = new();

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
            var totalAffection = GetAffectionByGroup(other) + GetAffection(other);

            return totalAffection > AffectionAllyThreshold;
        }

        public bool IsEnemy(IAffiliation other)
        {
            var totalAffection = GetAffectionByGroup(other) + GetAffection(other);

            return totalAffection < AffectionEnemyThreshold;
        }

        private readonly Dictionary<IAffiliation, float> affections = new();

        public void ModifyAffection(IAffiliation target, float change)
        {
            if (target == this)
            {
                return;
            }
            if (!affections.ContainsKey(target))
            {
                affections[target] = 0;
            }

            affections[target] += change;
            _onAffectionChanged.OnNext(new OnAffectionChangedMessage(target, affections[target], IsEnemy(target), IsAlly(target)));
        }

        public void UpdateTurn(IEnumerable<IAffiliation> visibleCharacters)
        {
            foreach (var target in affections.Keys.Where(target => !visibleCharacters.Contains(target)).ToList())
            {
                ModifyAffection(target, affections[target] * -0.001f);
                if (Mathf.Abs(affections[target]) <= 0.01f)
                {
                    affections.Remove(target);
                }
            }
        }

        private float GetAffectionByGroup(IAffiliation target)
        {
            if (target == this)
            {
                return 0;
            }
            return (Group, target.Group) switch
            {
                (CharacterGroup.Player, CharacterGroup.Player) => BaseAllyValue,
                (CharacterGroup.Player, CharacterGroup.Enemy) => BaseEnemyValue,
                (CharacterGroup.Enemy, CharacterGroup.Player) => BaseEnemyValue,
                (CharacterGroup.Enemy, CharacterGroup.Enemy) => 0,
                (CharacterGroup.Neutral, _) => 0,
                _ => 0,
            };
        }

        private float GetAffection(IAffiliation target)
        {
            if (target == this)
            {
                return 0;
            }
            if (affections.ContainsKey(target))
            {
                return affections[target];
            }
            return 0; // デフォルトの好感度は0とする
        }

        public void OnCharacterAttacked(IAffiliation attacker, IAffiliation target, float impact)
        {
            if (target == attacker)
            {
                return;
            }
            if (target == this)
            {
                ModifyAffection(attacker, -impact); // 攻撃されると好感度が減少
            }
            else if (attacker == this)
            {
                return;
            }
            else
            {
                if (IsAlly(target)) // 好感度が高い場合
                {
                    ModifyAffection(attacker, -impact); // 攻撃対象の好感度が高い場合、攻撃者に対する好感度を減少
                }
                else if (IsEnemy(target)) // 好感度が低い場合
                {
                    ModifyAffection(attacker, impact); // 攻撃対象の好感度が低い場合、攻撃者に対する好感度を増加
                }
                if (IsAlly(attacker))
                {
                    ModifyAffection(target, -impact);// 攻撃者が味方の場合、攻撃されるユーザーの好感度を減少
                }
                else if (IsEnemy(attacker))
                {
                    ModifyAffection(target, impact);// 攻撃者が敵の場合、攻撃されるユーザーの好感度を増加
                }
            }
        }
        public void OnCharacterHealed(IAffiliation healer, IAffiliation target, float impact)
        {
            if (target == healer)
            {
                return;
            }
            if (target == this)
            {
                ModifyAffection(healer, impact); // 回復されると好感度が増加
            }
            else if (healer == this)
            {
                return;
            }
            else
            {
                if (IsAlly(target)) // 好感度が高い場合
                {
                    ModifyAffection(healer, impact/2); // 回復対象の好感度が高い場合、回復者に対する好感度を増加
                }
                else if (IsEnemy(target)) // 好感度が低い場合
                {
                    ModifyAffection(healer, -impact/2); // 回復対象の好感度が低い場合、回復者に対する好感度を減少
                }
                if (IsAlly(healer))
                {
                    ModifyAffection(target, impact/2);// 回復者が味方の場合、回復されるユーザーの好感度を増加
                }
                else if (IsEnemy(healer))
                {
                    ModifyAffection(target, -impact/2);// 回復者が敵の場合、回復されるユーザーの好感度を減少
                }
            }
        }
    }
}

