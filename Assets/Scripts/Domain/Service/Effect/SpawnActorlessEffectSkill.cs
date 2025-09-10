#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.Setting;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    public class SpawnActorlessEffectSkill : ISerializable<SpawnActorlessEffectSkillMemento>, ISkill
    {
        private readonly IPositionOnlyDependentEffectPosition _position;
        private readonly INotDirectionalArea _area;
        private readonly List<IActorlessEffect> _effects;
        public int Repeats { get; private set; }
        public float ProbabilityOfSuccess { get; private set; }
        private readonly string? _log;

        public SpawnActorlessEffectSkill(SpawnActorlessEffectSkillMemento data)
        {
            _position = data.Position;
            _area = data.Area;
            _effects = data.Effects;
            Repeats = data.Repeats;
            ProbabilityOfSuccess = data.ProbabilityOfSuccess;
            _log = data.Log;
        }

        public Color Color => _effects.First().Color;
        public bool IsDirectional => _area.IsDirectional || _position.IsDirectional;

        public SpawnActorlessEffectSkillMemento Serialize()
        {
            return new SpawnActorlessEffectSkillMemento
            (
                _position,
                _area,
                _effects,
                Repeats,
                ProbabilityOfSuccess,
                _log
            );
        }

        public static SpawnActorlessEffectSkillMemento Build(IActorlessSkillData data)
        {
            return new SpawnActorlessEffectSkillMemento
            (
                data.Position,
                data.Area,
                data.Effects,
                data.Repeats,
                data.ProbabilityOfSuccess,
                data.Log
            );
        }

        public IEnumerable<Vector2Int> GetArea(Vector2Int position,
            IMap map)
        {
            var spawnPositions = _position.Get(position, map);

            return spawnPositions
                .SelectMany(spawnPosition => _area.Get(spawnPosition, map));
        }

        public async UniTask<ISkillResult> Use(string? name, Vector2Int position, IMap map)
        {
            if (_log != null && _log != "")
                GameLog.Add(map.Player.Character.IsVisible(position), $"{name}{_log}");

            if (Random.value > ProbabilityOfSuccess)
            {
                GameLog.Add(map.Player.Character.IsVisible(position), "しかし効果がなかった");
                return SpawnEffectSkillResult.Failed;
            }

            var area = GetArea(position, map);
            if (_effects.Any(effect =>
                effect is AttackEffect ||
                effect is AbsorbsEffect ||
                effect is PercentageDamageEffect ||
                effect is BreakEffect))
            {
                map.SetGrasses(area, false);
            }

            if (_effects.Any(effect =>
                effect is AttackEffect ||
                effect is AbsorbsEffect ||
                effect is PercentageDamageEffect))
            {
                map.AttackStatue(area);
            }

            for (var i = 0; i < Repeats; i++)
            {
                foreach (var effect in _effects)
                {
                    foreach (var target in map.Entities.In(area)
                                 .OrderBy(target => Vector2.Distance(target.Entity.CurrentPosition, position))
                                 .Reverse())
                    {
                        switch (target)
                        {
                            case ICharacter character:
                                await effect.Apply(character, position, map);
                                break;
                            default:
                                await effect.Apply(target, position, map);
                                break;
                        }
                    }

                    await effect.Apply(area, map);
                }

                if (map.Player.Character.VisibleArea.Intersect(area).Any())
                {
                    map.SpawnEffect(area, Color);
                    await UniTask.Delay(Settings.GlobalSettings.EffectDisplayTime.CurrentValue);
                }
            }

            return SpawnEffectSkillResult.Success(Color, area);
        }

        public float EvaluatePrice()
        {
            var price = 0f;
            foreach (var effect in _effects)
            {
                price += effect.EvaluatePrice();
            }
            price *= Repeats;

            price *= _area.EvaluateArea();
            price *= _position.EvaluateHitProbability();
            return price * ProbabilityOfSuccess;
        }

        public List<UpgradeData> GetUpgrades()
        {
            return new List<UpgradeData>();
        }

        public Dictionary<string, IHasUpgrades> GetChildren()
        {
            var children = new Dictionary<string, IHasUpgrades>();
            foreach (var effect in _effects)
            {
                children.Add(effect.UpgradePathName, effect);
            }

            children.Add(_position.UpgradePathName, _position);
            children.Add(_area.UpgradePathName, _area);
            return children;
        }

        public string InfoOnUse(bool omitProbabilityOfSuccess = false)
        {
            var info = "";
            if (Repeats > 1)
                info += $"効果は{Repeats}回発動する\n";
            info += $"{_position.Info()}の{_area.Info()}を対象にして\n";
            foreach (var (effect, index) in _effects.Index())
            {
                info += effect.Info();
            }
            if (!omitProbabilityOfSuccess)
                info += $"発動は{ProbabilityOfSuccess:P0}の確率で成功する\n";
            return info;
        }

        public string InfoOnThrow(bool omitEffects = false)
        {
            var info = "";
            if (Repeats > 1)
                info += $"効果は{Repeats}回発動する\n";
            info += $"{_position.Info()}の{_area.Info()}を対象にして\n";
            if (!omitEffects)
            {
                foreach (var (effect, index) in _effects.Index())
                {
                    info += effect.Info();
                }
            }
            else
            {
                info += "使用時と同じ効果を発揮する\n";
            }

            info += $"発動は{ProbabilityOfSuccess:P0}の確率で成功する\n";
            return info;
        }
    }
}