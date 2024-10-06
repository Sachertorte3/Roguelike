#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Type;
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
                Name,
                new Human("Chara_Hero1_USM"),
                new BehaviorMemento(
                    new BehaviorData(),
                    Option<Vector2Int>.None,
                    null
                ),
                CharacterStatusManager.Build(CommonSenseParameters.PlayerMaxHealth, 0.1f,
                    new Dictionary<Element, float>(), new Dictionary<Element, float>(), 10, 1, false),
                Entity.Build(spawnPosition, EntityLayer.Middle),
                Direction8.Down,
                new[]
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
                                        0,
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
                new InventoryMemento
                (
                    EnumerableExtension.CreateNewInstances<Option<ItemMemento>>(10).ToArray()
                ),
                CharacterAffiliationManager.Build(CharacterGroup.Human, null, null),
                Aggression.AttackAnyone,
                0,
                true,
                false,
                false,
                false,
                true,
                true
            );
        }

        public static CharacterMemento BuildCharacter(EnemyData data, Vector2Int spawnPosition,
            Direction8 direction = Direction8.Down, bool isSlept = false, bool isShiny = false,
            AffiliationMemento? affiliation = null, Id<IEntity>? id = null, Vector2Int? homePosition = null)
        {
            var inventory = new InventoryMemento
            (
                EnumerableExtension.CreateNewInstances<Option<ItemMemento>>(10).ToArray()
            );
            if (Random.value < data.DropItemRate)
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
                    homePosition.ToOption()
                ),
                CharacterStatusManager.Build(isShiny ? data.Hp * 10 : data.Hp, 0.1f,
                    isShiny
                        ? Enum.GetValues(typeof(Element)).Cast<Element>().ToDictionary(element => element, _ => 2f)
                        : new Dictionary<Element, float>(), data.ElementDamageRateMultiplier, 8,
                    data.MoveSpeed.ToWaitTime(), isSlept),
                Entity.Build(spawnPosition, EntityLayer.Middle),
                direction,
                data.Skills.Select(x => CharacterSkill.Build(SpawnEffectSkill.Build(x.Skill), x.CoolTime)).ToArray(),
                (data.HasLastSkill ? SpawnEffectSkill.Build(data.LastSkill) : null).ToOption(),
                inventory,
                CharacterAffiliationManager.Build(data.Group, affiliation, id),
                data.Aggression,
                0,
                false,
                isShiny,
                data.IsBoss,
                data.IsFlying,
                data.CanPickUp,
                data.CanUseItem
            );
        }

        public ICharacter CreatePlayer(CharacterMemento playerData, CharacterControlInputReceiver receiver,
            ReactiveProperty<bool> canIgnoreWall, IMap map)
        {
            return new Character(playerData, new PlayerBehavior(receiver), canIgnoreWall, map);
        }

        public ICharacter CreateCharacter(CharacterMemento data, ICharacterBehavior behavior,
            ReactiveProperty<bool> canIgnoreWall, IMap map)
        {
            return new Character(data, behavior, canIgnoreWall, map);
        }
    }
}