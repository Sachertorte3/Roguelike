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
                name: Name,
                characterType: new Human("Chara_Hero1_USM"),
                behavior: new BehaviorData(),
                homePosition: StructOption<Vector2Int>.None,
                status: CharacterStatusManager.Build(CommonSenseParameters.PlayerMaxHealth, 0.1f, new(), new(), 10, 1, false),
                entity: Entity.Build(spawnPosition, EntityLayer.Middle),
                direction: Direction8.Down,
                skills: new[]
                {
                    CharacterSkill.Build(SpawnEffectSkill.Build(new SkillData(new AtFeet(), new LineArea(1, false, false),
                        new AttackEffect(new List<ElementPower> { new(Element.Physical, 3) }, 0, new List<AdditionalConditionData>(), 0), 0, "は殴りかかった")),
                        0
                    )
                },
                lastSkill: new(null),
                inventory: new InventoryMemento
                (
                    items: EnumerableExtension.CreateNewInstances<Option<ItemMemento>>(10).ToArray()
                ),
                affiliation: CharacterAffiliationManager.Build(CharacterGroup.Human, null, null),
                aggression: Aggression.AttackAnyone,
                money: 0,
                isLeader: true,
                isShiny: false,
                isBoss: false,
                canPickUp: true,
                canUseItem: true
            );
        }

        public static CharacterMemento BuildCharacter(EnemyData data, Vector2Int spawnPosition, bool isSlept, bool isShiny, AffiliationMemento? affiliation = null, Id<IEntity>? id = null, bool hasHomePosition = false)
        {
            var inventory = new InventoryMemento
            (
                items: EnumerableExtension.CreateNewInstances<Option<ItemMemento>>(10).ToArray()
            );
            if (Random.value < data.DropItemRate)
            {
                var dropItem = data.DropItemTable.GetRandomItem();
                inventory.Items[0] = new Option<ItemMemento>(Item.Build(dropItem));
            }
            return new CharacterMemento
            (
                name: isShiny ? "☆" + data.Name : data.Name,
                characterType: data.CharacterType,
                behavior: data.Behavior,
                homePosition: hasHomePosition ? new(spawnPosition) : StructOption<Vector2Int>.None,
                status: CharacterStatusManager.Build(isShiny ? data.Hp * 10 : data.Hp, 0.1f, isShiny ? Enum.GetValues(typeof(Element)).Cast<Element>().ToDictionary(element => element, _ => 2f) : new Dictionary<Element, float>(), data.ElementDamageRateMultiplier, 8, data.MoveSpeed.ToWaitTime(), isSlept),
                entity: Entity.Build(spawnPosition, EntityLayer.Middle),
                direction: Direction8.Down,
                skills: data.Skills.Select(x => CharacterSkill.Build(SpawnEffectSkill.Build(x.Skill), x.CoolTime)).ToArray(),
                lastSkill: new Option<SpawnEffectSkillMemento>(data.HasLastSkill ? SpawnEffectSkill.Build(data.LastSkill) : null),
                inventory: inventory,
                affiliation: CharacterAffiliationManager.Build(data.Group, affiliation, id),
                aggression: data.Aggression,
                money: 0,
                isLeader: false,
                isShiny: isShiny,
                isBoss: data.IsBoss,
                canPickUp: data.CanPickUp,
                canUseItem: data.CanUseItem
            );
        }

        public ICharacter CreatePlayer(CharacterMemento playerData, CharacterControlInputReceiver receiver,
            ReactiveProperty<bool> canIgnoreWall, IMap world)
        {
            return new Character(playerData, new PlayerBehavior(receiver), canIgnoreWall, world);
        }

        public ICharacter CreateCharacter(CharacterMemento data, ICharacterBehavior behavior,
            ReactiveProperty<bool> canIgnoreWall, IMap world)
        {
            return new Character(data, behavior, canIgnoreWall, world);
        }
    }
}