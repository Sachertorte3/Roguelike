#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Character.Type;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Behavior;
using Domain.Service.Effect;
using Domain.Service.Items;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Characters
{
    public sealed class CharacterFactory
    {
        public static PlayerMemento BuildPlayer(string Name, Vector2Int spawnPosition)
        {
            var character = new CharacterMemento
            (
                Name,
                new Human("Chara_Hero1_USM"),
                PlayerBehavior.Build(),
                CharacterStatusManager.Build(CommonSenseParameters.PlayerMaxHealth,
                    CommonSenseParameters.PlayerNaturalRecoveryRate,
                    new Dictionary<Element, float>(), new Dictionary<Element, float>(),
                    new Dictionary<ConditionTemplate, float>(), CommonSenseParameters.PlayerVisionRange, false, false, true, 1, false),
                EntityBase.Build(spawnPosition, EntityLayer.Middle),
                Direction8.Down,
                new List<CharacterSkillMemento>
                {
                    CharacterSkill.Build(
                        SpawnEffectSkill.Build(
                            new SkillData(
                                new AtFeet(),
                                new LineArea(1, false, false),
                                new List<IEffect>
                                {
                                    new AttackEffect(
                                        new List<ElementPower> { new(Element.Physical, CommonSenseParameters.PlayerAttackPowerWhenUnarmed) },
                                        0
                                    )
                                },
                                1,
                                CommonSenseParameters.SkillOnUseProbabilityOfSuccess,
                                "は殴りかかった")
                        ),
                        0,
                        0,
                        0,
                        0
                    )
                },
                Option<SpawnEffectSkillMemento>.None,
                Storage.Build(10, true, true),
                new List<string>(),
                CharacterAffiliationManager.Build(CharacterGroup.Human),
                Aggression.AttackAnyone,
                0,
                true,
                false,
                false,
                false,
                false,
                true,
                true
            );
            return new PlayerMemento(character, 0);
        }

        public static CharacterMemento BuildCharacter(EnemyData data, Vector2Int spawnPosition,
            Direction8 direction = Direction8.Down, bool isSlept = false, bool isShiny = false,
            IAffiliation? affiliation = null, Location? homeLocation = null)
        {
            var inventory = Storage.Build(10, true, true);
            if (RandUtils.IsLessThanProbability(data.DropItemRate) && data.DropItemTable.Count > 0)
            {
                var dropItem = data.DropItemTable.GetRandomItem();
                inventory.Items[0] = ((IItemMemento)Item.Build(dropItem)).ToOption();
            }

            return new CharacterMemento
            (
                isShiny ? "☆" + data.Name : data.Name,
                data.CharacterType,
                EnemyBehavior.Build(
                    data.Behavior,
                    homeLocation.ToOption()
                ),
                CharacterStatusManager.Build(isShiny ? data.Hp * 10 : data.Hp, 0.1f,
                    isShiny
                        ? Enum.GetValues(typeof(Element)).Cast<Element>().ToDictionary(element => element, _ => 2f)
                        : new Dictionary<Element, float>(), data.ElementDamageRateMultiplier, data.ConditionResistance,
                    8, data.IsHard,
                    data.IsHeavy, false, data.MoveSpeed.ToWaitTime(), isSlept),
                EntityBase.Build(spawnPosition, EntityLayer.Middle),
                direction,
                data.Skills.Select(x => CharacterSkill.Build(x)).ToList(),
                (data.HasLastSkill ? SpawnEffectSkill.Build(data.LastSkill) : null).ToOption(),
                inventory,
                new List<string>(),
                CharacterAffiliationManager.Build(data.Group, affiliation),
                data.Aggression,
                EvaluateExp(data, isShiny),
                false,
                isShiny,
                data.IsBoss,
                data.IsFlying,
                data.CanThroughWalls,
                data.CanPickUp,
                data.CanUseItem
            );
        }

        public static IPlayer CreatePlayer(PlayerMemento playerData, CharacterControlInputReceiver receiver, IGameManager gameManager, IMap map)
        {
            return new Player(playerData, receiver, gameManager, map);
        }

        public static ICharacter CreateCharacter(CharacterMemento data, ICharacterBehavior behavior, IGameManager gameManager, IMap map)
        {
            return new Character(data, behavior, gameManager, map, false);
        }

        public static int EvaluateExp(EnemyData enemyData, bool isShiny)
        {
            var value = 1.0f;
            value *= isShiny ? enemyData.Hp * 10 : enemyData.Hp;
            value /= EvaluateDamageRate(enemyData);

            value *= isShiny ? EvaluateSkills(enemyData.Skills) * 2 : EvaluateSkills(enemyData.Skills);

            value *= enemyData.MoveSpeed switch
            {
                MoveSpeed.Quarter => 0.25f,
                MoveSpeed.Half => 0.5f,
                MoveSpeed.Normal => 1.0f,
                MoveSpeed.Double => 2.0f,
                MoveSpeed.Quadruple => 4.0f,
                _ => throw new ArgumentException($"Invalid MoveSpeed: {enemyData.MoveSpeed}"),
            };
            if (enemyData.Behavior.Default == MoveTypeWhenDiscoveringTarget.NoMove)
            {
                value *= 0.5f;
            }
            if (enemyData.IsHard)
            {
                value *= 5.0f;
            }
            if (enemyData.IsHeavy)
            {
                value *= 1.2f;
            }
            if (enemyData.IsFlying)
            {
                if (enemyData.CanThroughWalls)
                {
                    value *= 1.5f;
                }
                else
                {
                    value *= 1.2f;
                }
            }
            if (enemyData.CanPickUp)
            {
                value *= 1.1f;
            }
            if (enemyData.CanUseItem)
            {
                value *= 1.5f;
            }
            return Mathf.RoundToInt(value);
        }
        public static float EvaluateDamageRate(EnemyData enemyData)
        {
            var sum = 0f;
            foreach (var element in Enum.GetValues(typeof(Element)))
            {
                if (enemyData.ElementDamageRateMultiplier.TryGetValue((Element)element, out var multiplier))
                {
                    sum += multiplier;
                }
                else
                {
                    sum += 1;
                }
            }
            return sum / Enum.GetValues(typeof(Element)).Length;
        }
        public static float EvaluateSkills(IEnumerable<EnemySkillData> skills)
        {
            var virtualSkills = skills.Select(x => new VirtualSkill(x)).ToList();
            var sum = 0f;
            var turn = 0;
            while (turn < 100)
            {
                foreach (var skill in virtualSkills)
                {
                    skill.Update(1);
                }
                var selectedSkill = virtualSkills.Where(x => x.IsReady()).MaxByOrDefault(x => x.Value, null);
                if (selectedSkill == null)
                {
                    turn++;
                    continue;
                }
                if (selectedSkill.ChargeTurn > 0)
                {
                    foreach (var skill in virtualSkills)
                    {
                        skill.Update(selectedSkill.ChargeTurn);
                    }

                    sum += selectedSkill.Value * selectedSkill.ChargeTurn;
                    turn += selectedSkill.ChargeTurn;
                }
                selectedSkill.Use();
                sum += selectedSkill.Value;
                turn++;
            }
            return sum / turn;
        }
        private class VirtualSkill
        {
            public float Value;
            public int ChargeTurn;
            public int CoolTime;
            private int _remainingCoolTime;
            public VirtualSkill(EnemySkillData skill)
            {
                Value = new CharacterSkill(CharacterSkill.Build(skill)).EvaluatePrice();
                ChargeTurn = skill.ChargeTurn;
                CoolTime = skill.CoolTime;
                _remainingCoolTime = 0;
            }
            public void Update(int turnCount)
            {
                _remainingCoolTime = Mathf.Max(0, _remainingCoolTime - turnCount);
            }
            public void Use()
            {
                _remainingCoolTime = 1 + CoolTime;
            }
            public bool IsReady()
            {
                return _remainingCoolTime == 0;
            }
        }
    }
}