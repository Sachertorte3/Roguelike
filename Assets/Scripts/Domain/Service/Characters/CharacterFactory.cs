#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Character.Type;
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
using Utilities.Serialize;
using Random = UnityEngine.Random;

namespace Domain.Service.Characters
{
    public sealed class CharacterFactory
    {
        public static CharacterMemento BuildPlayer(string Name, Vector2Int spawnPosition)
        {
            return new CharacterMemento
            (
                Name,
                new Human("Chara_Hero1_USM"),
                PlayerBehavior.Build(),
                CharacterStatusManager.Build(CommonSenseParameters.PlayerMaxHealth,
                    CommonSenseParameters.PlayerNaturalRecoveryRate,
                    new Dictionary<Element, float>(), new Dictionary<Element, float>(),
                    new Dictionary<ConditionTemplate, float>(), 10, false, false, true, 1, false),
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
                                        new List<ElementPower> { new(Element.Physical, 3) },
                                        0
                                    )
                                },
                                0,
                                0,
                                "は殴りかかった")
                        ),
                        0
                    )
                },
                Option<SpawnEffectSkillMemento>.None,
                Storage.Build(10),
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
        }

        public static CharacterMemento BuildCharacter(EnemyData data, Vector2Int spawnPosition,
            Direction8 direction = Direction8.Down, bool isSlept = false, bool isShiny = false,
            IAffiliation? affiliation = null, (Location, Vector2Int)? homePosition = null)
        {
            var inventory = Storage.Build(10);
            if (Random.value < data.DropItemRate && data.DropItemTable.Count > 0)
            {
                var dropItem = data.DropItemTable.GetRandomItem();
                inventory.Items[0] = Item.Build(dropItem).ToOption();
            }

            return new CharacterMemento
            (
                isShiny ? "☆" + data.Name : data.Name,
                data.CharacterType,
                EnemyBehavior.Build(
                    data.Behavior,
                    homePosition
                ),
                CharacterStatusManager.Build(isShiny ? data.Hp * 10 : data.Hp, 0.1f,
                    isShiny
                        ? Enum.GetValues(typeof(Element)).Cast<Element>().ToDictionary(element => element, _ => 2f)
                        : new Dictionary<Element, float>(), data.ElementDamageRateMultiplier, data.ConditionResistance,
                    8, data.IsHard,
                    data.IsHeavy, false, data.MoveSpeed.ToWaitTime(), isSlept),
                EntityBase.Build(spawnPosition, EntityLayer.Middle),
                direction,
                data.Skills.Select(x => CharacterSkill.Build(SpawnEffectSkill.Build(x.Skill), x.CoolTime)).ToList(),
                (data.HasLastSkill ? SpawnEffectSkill.Build(data.LastSkill) : null).ToOption(),
                inventory,
                new List<string>(),
                CharacterAffiliationManager.Build(data.Group, affiliation),
                data.Aggression,
                0,
                false,
                isShiny,
                data.IsBoss,
                data.IsFlying,
                data.CanThroughWalls,
                data.CanPickUp,
                data.CanUseItem
            );
        }

        public IPlayer CreatePlayer(CharacterMemento playerData, CharacterControlInputReceiver receiver, IMap map)
        {
            return new Player(playerData, receiver, map);
        }

        public ICharacter CreateCharacter(CharacterMemento data, ICharacterBehavior behavior, IMap map)
        {
            return new Character(data, behavior, map, false);
        }
    }
}