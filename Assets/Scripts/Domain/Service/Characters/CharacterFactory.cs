#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Type;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Behavior;
using Domain.Service.Effect;
using Domain.Service.Entities;
using Domain.Service.Items;
using R3;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace Domain.Service.Characters
{
    public sealed class CharacterFactory
    {
        public static CharacterMemento BuildPlayer(string Name, Vector2Int spawnPosition)
        {
            return new CharacterMemento
            (
                name: Name,
                characterType: new Human("Chara_Hero1_USM"),
                behavior: new BehaviorMemento(
                    new BehaviorData(),
                    null,
                    null
                ),
                status: CharacterStatusManager.Build(CommonSenseParameters.PlayerMaxHealth, 0.1f,
                    new(), new(), new(), 10, false, false, 1, false),
                entity: Entity.Build(spawnPosition, EntityLayer.Middle),
                direction: Direction8.Down,
                skills: new[]
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
                lastSkill: Option<SpawnEffectSkillMemento>.None,
                inventory: new InventoryMemento
                (
                    EnumerableExtension.CreateNewInstances<Option<ItemMemento>>(10).ToArray()
                ),
                knownItemNames: new List<string>(),
                affiliation: CharacterAffiliationManager.Build(CharacterGroup.Human),
                aggression: Aggression.AttackAnyone,
                money: 0,
                isLeader: true,
                isShiny: false,
                isBoss: false,
                isFlying: false,
                canThroughWalls: false,
                canPickUp: true,
                canUseItem: true
            );
        }

        public static CharacterMemento BuildCharacter(EnemyData data, ItemDatabase itemDatabase, Vector2Int spawnPosition,
            Direction8 direction = Direction8.Down, bool isSlept = false, bool isShiny = false,
            IAffiliation? affiliation = null, (Location, Vector2Int)? homePosition = null)
        {
            var inventory = new InventoryMemento
            (
                EnumerableExtension.CreateNewInstances<Option<ItemMemento>>(10).ToArray()
            );
            if (Random.value < data.DropItemRate && data.DropItemTable.Count > 0)
            {
                var dropItem = data.DropItemTable.GetRandomItem();
                inventory.Items[0] = Item.Build(dropItem, itemDatabase.GetPlaceholder(dropItem)).ToOption();
            }

            return new CharacterMemento
            (
                name: isShiny ? "☆" + data.Name : data.Name,
                characterType: data.CharacterType,
                behavior: EnemyBehavior.Build(
                    data.Behavior,
                    homePosition
                ),
                status: CharacterStatusManager.Build(isShiny ? data.Hp * 10 : data.Hp, 0.1f,
                    isShiny
                        ? Enum.GetValues(typeof(Element)).Cast<Element>().ToDictionary(element => element, _ => 2f)
                        : new Dictionary<Element, float>(), data.ElementDamageRateMultiplier, data.ConditionResistance, 8, data.IsHard,
                    data.IsHeavy, data.MoveSpeed.ToWaitTime(), isSlept),
                entity: Entity.Build(spawnPosition, EntityLayer.Middle),
                direction: direction,
                skills: data.Skills.Select(x => CharacterSkill.Build(SpawnEffectSkill.Build(x.Skill), x.CoolTime)).ToArray(),
                lastSkill: (data.HasLastSkill ? SpawnEffectSkill.Build(data.LastSkill) : null).ToOption(),
                inventory: inventory,
                knownItemNames: new List<string>(),
                affiliation: CharacterAffiliationManager.Build(data.Group, affiliation),
                aggression: data.Aggression,
                money: 0,
                isLeader: false,
                isShiny: isShiny,
                isBoss: data.IsBoss,
                isFlying: data.IsFlying,
                canThroughWalls: data.CanThroughWalls,
                canPickUp: data.CanPickUp,
                canUseItem: data.CanUseItem
            );
        }

        public ICharacter CreatePlayer(CharacterMemento playerData, CharacterControlInputReceiver receiver, IMap map)
        {
            return new Character(playerData, new PlayerBehavior(receiver), map);
        }

        public ICharacter CreateCharacter(CharacterMemento data, ICharacterBehavior behavior, IMap map)
        {
            return new Character(data, behavior, map);
        }
    }
}