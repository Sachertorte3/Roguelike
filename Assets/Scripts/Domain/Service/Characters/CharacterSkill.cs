#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Effect;
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
            {
                Skill = _skill.Serialize(),
                CoolTime = _coolTime,
                RemainingTurn = _remainingCoolTime
            };
        }
        public static CharacterSkillMemento Build(SpawnEffectSkillMemento skill, int coolTime)
        {
            return new CharacterSkillMemento
            {
                Skill = skill,
                CoolTime = coolTime,
                RemainingTurn = 0
            };
        }
        public IEnumerable<Vector2Int> GetArea(IActorOfEffect actor, Vector2Int position, Direction8 direction, IEffectMap map) =>
            _skill.GetArea(actor, position, direction, map);
        public string Info() => _skill.Info();
        public UniTask<bool> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            _remainingCoolTime = _coolTime + 1;
            return _skill.Use(actor, position, direction, map);
        }
        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            return _skill.Evaluate(actor, position, direction, world);
        }
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