#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Effect;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters
{
    public class CharacterSkill : ICharacterSkill
    {
        public SpawnEffectSkill _skill { get; }
        private int _coolTime { get; }
        public Color Color => _skill.Color;
        public int RushDistance => _skill.RushDistance;
        private int _remainingCoolTime;
        public CharacterSkill(CharacterSkillMemento data)
        {
            _skill = new SpawnEffectSkill(data.Skill);
            _coolTime = data.CoolTime;
            _remainingCoolTime = data.RemainingTurn;
        }
        public CharacterSkillMemento Serialize()
        {
            return new CharacterSkillMemento
            (
                skill: _skill.Serialize(),
                coolTime: _coolTime,
                remainingTurn: _remainingCoolTime
            );
        }
        public static CharacterSkillMemento Build(SpawnEffectSkillMemento skill, int coolTime)
        {
            return new CharacterSkillMemento
            (
                skill: skill,
                coolTime: coolTime,
                remainingTurn: 0
            );
        }
        public string Info() => _skill.InfoOnUse();
        public UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            _remainingCoolTime = _coolTime + 1;
            return _skill.Use(actor, position, direction, map);
        }
        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            return _skill.Evaluate(actor, position, direction, world);
        }
        public float EvaluatePrice() => _skill.EvaluatePrice();
        public Dictionary<UpgradePath, UpgradeData> GetUpgrades() => _skill.GetUpgrades();
        public void UpdateTurn()
        {
            if (_remainingCoolTime > 0)
            {
                _remainingCoolTime--;
            }
        }
        public bool IsUsable()
        {
            return _remainingCoolTime <= 0;
        }
    }
}