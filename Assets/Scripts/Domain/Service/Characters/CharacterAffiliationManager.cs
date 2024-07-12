using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Effect;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters
{
    public class CharacterAffiliationManager : IAffiliation, ISerializable<AffiliationMemento>
    {
        private const float AffectionAllyThreshold = 1f; // 味方と見なす好感度の閾値
        private const float AffectionEnemyThreshold = 0f; // 敵と見なす好感度の閾値
        private const float BaseAllyValue = 1.2f; // 味方グループの基本好感度
        private const float BaseEnemyValue = -1f; // 敵対グループの基本好感度

        private readonly Dictionary<int, float> _affections;
        private readonly int _id;
        private readonly Subject<OnAffectionChangedMessage> _onAffectionChanged = new();

        public CharacterAffiliationManager(AffiliationMemento data)
        {
            _id = data.Id;
            Group = data.Group;
            _affections = data.Affiliations.Select(x => (x.Key, x.Value)).ToDictionary(x => x.Item1, x => x.Item2);
        }

        public int Id => _id;
        public Observable<OnAffectionChangedMessage> OnAffectionChanged => _onAffectionChanged;

        public CharacterGroup Group { get; private set; }

        public bool IsAlly(IAffiliation other)
        {
            if (other.Id == Id)
            {
                return true;
            }

            var totalAffection = GetAffectionByGroup(other) + GetAffection(other.Id);

            return totalAffection > AffectionAllyThreshold;
        }

        public bool IsEnemy(IAffiliation other)
        {
            if (other.Id == Id)
            {
                return false;
            }

            var totalAffection = GetAffectionByGroup(other) + GetAffection(other.Id);

            return totalAffection < AffectionEnemyThreshold;
        }

        public void OnCharacterAttacked(IAffiliation attacker, IAffiliation target, float impact)
        {
            impact += 0.2f;
            if (target.Id == attacker.Id)
            {
                return;
            }

            if (target == this)
            {
                ModifyAffection(attacker.Id, -impact); // 攻撃されると好感度が減少
            }
            else if (attacker.Id == Id)
            {
                return;
            }
            else
            {
                if (IsAlly(target)) // 好感度が高い場合
                {
                    ModifyAffection(attacker.Id, -impact); // 攻撃対象の好感度が高い場合、攻撃者に対する好感度を減少
                }
                else if (IsEnemy(target)) // 好感度が低い場合
                {
                    ModifyAffection(attacker.Id, impact); // 攻撃対象の好感度が低い場合、攻撃者に対する好感度を増加
                }

                if (IsAlly(attacker))
                {
                    ModifyAffection(target.Id, -impact); // 攻撃者が味方の場合、攻撃されるユーザーの好感度を減少
                }
                else if (IsEnemy(attacker))
                {
                    ModifyAffection(target.Id, impact); // 攻撃者が敵の場合、攻撃されるユーザーの好感度を増加
                }
            }
        }

        public void OnCharacterHealed(IAffiliation healer, IAffiliation target, float impact)
        {
            impact += 0.2f;
            if (target == healer)
            {
                return;
            }

            if (target == this)
            {
                ModifyAffection(healer.Id, impact); // 回復されると好感度が増加
            }
            else if (healer == this)
            {
                return;
            }
            else
            {
                if (IsAlly(target)) // 好感度が高い場合
                {
                    ModifyAffection(healer.Id, impact / 2); // 回復対象の好感度が高い場合、回復者に対する好感度を増加
                }
                else if (IsEnemy(target)) // 好感度が低い場合
                {
                    ModifyAffection(healer.Id, -impact / 2); // 回復対象の好感度が低い場合、回復者に対する好感度を減少
                }

                if (IsAlly(healer))
                {
                    ModifyAffection(target.Id, impact / 2); // 回復者が味方の場合、回復されるユーザーの好感度を増加
                }
                else if (IsEnemy(healer))
                {
                    ModifyAffection(target.Id, -impact / 2); // 回復者が敵の場合、回復されるユーザーの好感度を減少
                }
            }
        }

        public AffiliationMemento Serialize()
        {
            return new AffiliationMemento(
                Id,
                Group,
                _affections.Select(x => (x.Key, x.Value)).ToDictionary(x => x.Item1, x => x.Item2)
            );
        }

        public static AffiliationMemento Build(CharacterGroup group)
        {
            return new AffiliationMemento(
                UniqueIdGenerator.Generate<IAffiliation>().Value,
                group,
                new Dictionary<int, float>()
            );
        }

        public void ModifyAffection(int targetId, float change)
        {
            if (targetId == Id)
            {
                return;
            }

            if (!_affections.ContainsKey(targetId))
            {
                _affections[targetId] = 0;
            }

            _affections[targetId] += change;
            _onAffectionChanged.OnNext(new OnAffectionChangedMessage(targetId, _affections[targetId]));
        }

        public void UpdateTurn(IEnumerable<IAffiliation> visibleCharacters)
        {
            foreach (var target in _affections.Keys
                         .Where(target => !visibleCharacters.Select(x => x.Id).Contains(target)).ToList())
            {
                ModifyAffection(target, _affections[target] * -0.001f);
                if (Mathf.Abs(_affections[target]) <= 0.01f)
                {
                    _affections.Remove(target);
                }
            }
        }

        private float GetAffectionByGroup(IAffiliation target)
        {
            if (target.Id == Id)
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
                _ => 0
            };
        }

        private float GetAffection(int target)
        {
            if (target == Id)
            {
                return 0;
            }

            if (_affections.ContainsKey(target))
            {
                return _affections[target];
            }

            return 0; // デフォルトの好感度は0とする
        }
    }
}