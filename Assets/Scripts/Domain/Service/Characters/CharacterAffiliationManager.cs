#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Memento;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters
{
    public class CharacterAffiliationManager : IAffiliation, ISerializable<AffiliationMemento>
    {
        private const float AffectionAllyThreshold = 2f; // 味方と見なす好感度の閾値
        private const float AffectionEnemyThreshold = 0f; // 敵と見なす好感度の閾値
        private const float BaseAllyValue = 1f; // 味方グループの基本好感度
        private const float BaseEnemyValue = -1f; // 敵対グループの基本好感度

        private readonly Dictionary<Id<IEntity>, float> _affections;
        private readonly Id<IEntity> _id;
        private readonly Subject<OnAffiliationChangedMessage> _OnAffiliationChanged = new();
        private IAffiliation? _player;
        private readonly Dictionary<Id<IEntity>, AffiliationType> _forcedAffiliation;

        public CharacterAffiliationManager(Id<IEntity> id, AffiliationMemento data, IAffiliation? player)
        {
            _id = id;
            Group = data.Group;
            _affections = data.Affiliations;
            _forcedAffiliation = data.ForcedAffiliations;
            _player = player;
        }

        public Id<IEntity> Id => _id;
        public Observable<OnAffiliationChangedMessage> OnAffiliationChanged => _OnAffiliationChanged;

        public CharacterGroup Group { get; private set; }

        public AffiliationType GetAffiliationType(IAffiliation other)
        {
            if (_forcedAffiliation.ContainsKey(other.Id))
            {
                return _forcedAffiliation[other.Id];
            }

            if (other.Id == Id)
            {
                return AffiliationType.Ally;
            }

            if (other != _player && _player != null && IsAlly(_player))
            {
                return other.GetAffiliationType(_player);
            }

            var totalAffection = GetAffection(other);

            return totalAffection switch
            {
                > AffectionAllyThreshold => AffiliationType.Ally,
                < AffectionEnemyThreshold => AffiliationType.Enemy,
                _ => AffiliationType.Neutral
            };
        }

        public bool IsAlly(IAffiliation other) => GetAffiliationType(other) == AffiliationType.Ally;
        public bool IsEnemy(IAffiliation other) => GetAffiliationType(other) == AffiliationType.Enemy;

        public void OnCharacterAttacked(IAffiliation attacker, IAffiliation target, float impact)
        {
            impact += 0.2f;
            if (target.Id == attacker.Id)
            {
                return;
            }

            if (target.Id == Id)
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
            return new AffiliationMemento
            (
                group: Group,
                affiliations: _affections,
                forcedAffiliations: _forcedAffiliation
            );
        }

        public static AffiliationMemento Build(CharacterGroup group, AffiliationMemento? affiliation, Id<IEntity>? id)
        {

            var affiliationDict = new Dictionary<Id<IEntity>, float>();
            if (affiliation != null && id != null)
            {
                affiliationDict = affiliation.Affiliations;
                affiliationDict[id] = 5f;
            }
            return new AffiliationMemento
            (
                group: group,
                affiliations: affiliationDict,
                forcedAffiliations: new()
            );
        }

        public void ModifyAffection(Id<IEntity> targetId, float change)
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
            _OnAffiliationChanged.OnNext(new OnAffiliationChangedMessage(targetId));
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

        public float GetAffection(IAffiliation target)
        {
            return GetAffectionByGroup(target) + GetAffectionByRelation(target.Id);
        }

        public void ForceAffiliation(IAffiliation target, AffiliationType type)
        {
            if (target.Id == Id)
            {
                return;
            }

            _forcedAffiliation[target.Id] = type;
            _OnAffiliationChanged.OnNext(new OnAffiliationChangedMessage(target.Id));
        }

        private float GetAffectionByGroup(IAffiliation target)
        {
            if (target.Id == Id)
            {
                return 0;
            }

            return (Group, target.Group) switch
            {
                (CharacterGroup.Human, CharacterGroup.Human) => BaseAllyValue,
                (CharacterGroup.Human, CharacterGroup.Monster) => BaseEnemyValue,
                (CharacterGroup.Monster, CharacterGroup.Human) => BaseEnemyValue,
                (CharacterGroup.Monster, CharacterGroup.Monster) => 0,
                (CharacterGroup.Neutral, _) => 0,
                _ => 0
            };
        }

        private float GetAffectionByRelation(Id<IEntity> target)
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