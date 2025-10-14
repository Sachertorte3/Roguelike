#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Character.Status;
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
        public bool IsDirectional => _skill.IsDirectional;
        public Color Color => _skill.Color;
        public int RushDistance { get; private set; }
        public int BackStepDistance { get; private set; }
        public int ChargeTurn { get; private set; }
        private readonly int _coolTime;
        private int _remainingCoolTime;

        public CharacterSkill(CharacterSkillMemento data)
        {
            _skill = new SpawnEffectSkill(data.Skill);
            _remainingCoolTime = data.RemainingTurn;
            RushDistance = data.RushDistance;
            BackStepDistance = data.BackStepDistance;
            ChargeTurn = data.ChargeTurn;
            _coolTime = data.CoolTime;
        }

        public CharacterSkillMemento Serialize()
        {
            return new CharacterSkillMemento
            (
                _skill.Serialize(),
                RushDistance,
                BackStepDistance,
                ChargeTurn,
                _coolTime,
                _remainingCoolTime
            );
        }

        public static CharacterSkillMemento Build(CharacterSkillData skill)
        {
            return Build(
                SpawnEffectSkill.Build(skill.Skill),
                skill.RushDistance,
                skill.BackStepDistance,
                skill.ChargeTurn,
                skill.CoolTime
            );
        }

        public static CharacterSkillMemento Build(SpawnEffectSkillMemento skill, int rushDistance, int backStepDistance, int chargeTurn, int coolTime)
        {
            return new CharacterSkillMemento
            (
                skill,
                rushDistance,
                backStepDistance,
                chargeTurn,
                coolTime,
                0
            );
        }

        public string Info()
        {
            return _skill.InfoOnUse();
        }

        public IEnumerable<Vector2Int> GetArea(IActor actor, Vector2Int position, Direction8 direction, IMap map, bool onlyVisible = false)
        {
            for (var i = 0; i < RushDistance; i++)
            {
                if (actor.CanMove(position, direction, map) && !actor.Status.IsFlagStat(FlagStatType.CannotMove))
                    position += direction.Vector();
                else
                    break;
            }
            return _skill.GetArea(actor, position, direction, map, onlyVisible);
        }

        public UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            _remainingCoolTime = _coolTime + 1;
            return _skill.Use(actor, position, direction, map);
        }

        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            for (var i = 0; i < RushDistance; i++)
            {
                if (actor.CanMove(position, direction, map) && !actor.Status.IsFlagStat(FlagStatType.CannotMove))
                    position += direction.Vector();
            }

            return _skill.Evaluate(actor, position, direction, map) / (1 + ChargeTurn);
        }

        public float EvaluatePrice()
        {
            return _skill.EvaluatePrice() / (1 + ChargeTurn);
        }

        public List<UpgradeData> GetUpgrades()
        {
            return _skill.GetUpgrades();
        }

        public Dictionary<string, IHasUpgrades> GetChildren()
        {
            return _skill.GetChildren();
        }

        public void CoolDown()
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

        public string InfoOnUse(bool omitProbabilityOfSuccess = false)
        {
            var info = "";
            if (RushDistance > 0)
                info += $"最初に{RushDistance}マス前に進む\n";

            info += _skill.InfoOnUse(omitProbabilityOfSuccess);

            if (BackStepDistance > 0)
                info += $"最後に{BackStepDistance}マス後ろに下がる\n";

            if (ChargeTurn > 0)
                info += $"発動には{ChargeTurn}ターンかかる\n";

            if (_coolTime > 0)
                info += $"発動後に{_coolTime}ターンは再使用不能\n";

            return info;
        }
    }
}