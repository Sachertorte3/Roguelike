#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Effect;
using Domain.Model.Item;
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
        public bool IsDirectional => _skill.IsDirectional;
        public Color Color => _skill.Color;
        public int RushDistance => _skill.RushDistance;
        public int BackStepDistance => _skill.BackStepDistance;
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
                _skill.Serialize(),
                _coolTime,
                _remainingCoolTime
            );
        }

        public static CharacterSkillMemento Build(SpawnEffectSkillMemento skill, int coolTime)
        {
            return new CharacterSkillMemento
            (
                skill,
                coolTime,
                0
            );
        }

        public string Info()
        {
            return _skill.InfoOnUse();
        }

        public UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            _remainingCoolTime = _coolTime + 1;
            return _skill.Use(actor, position, direction, map);
        }

        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            return _skill.Evaluate(actor, position, direction, map);
        }

        public float EvaluatePrice()
        {
            return _skill.EvaluatePrice();
        }

        public string UpgradePathName => "スキル";
        public List<UpgradeData> GetUpgrades() => _skill.GetUpgrades();
        public List<IHasUpgrades> GetChildren() => _skill.GetChildren();

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