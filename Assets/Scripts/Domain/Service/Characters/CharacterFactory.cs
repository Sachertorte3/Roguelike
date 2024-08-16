#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Effect.Area;
using Domain.Model.Character;
using Domain.Model.Character.Type;
using Domain.Model.Effect;
using Domain.Model.Effect.Position;
using Domain.Service.Characters.Behavior;
using Domain.Service.Effect;
using R3;
using UnityEngine;
using Domain.Service.Entities;
using Utilities;

namespace Domain.Service.Characters
{
    public sealed class CharacterFactory
    {
        public static CharacterMemento BuildPlayer(string Name, Vector2Int spawnPosition)
        {
            return new CharacterMemento
            {
                Name = Name,
                CharacterType = new Human("Chara_Hero1_USM"),
                Behavior = new BehaviorData(),
                Status = CharacterStatusManager.Build(100, 1, 1, new(), new(), 10, 1, false),
                Entity = Entity.Build(spawnPosition, EntityLayer.Middle),
                Direction = Direction8.Down,
                Skills = new[]
                {
                    CharacterSkill.Build(SpawnEffectSkill.Build(new SkillData(new AtFeet(), new LineArea(1, false),
                        new AttackEffect(new List<ElementPower> { new ElementPower(Element.Physical, 1) }, 0, new List<AdditionalConditionData>(), 0), 0, "は殴りかかった")),
                        0
                    )
                },
                LastSkill = new(null),
                Inventory = new InventoryMemento
                {
                    Items = EnumerableExtension.CreateNewInstances<Option<ItemMemento>>(10).ToArray()
                },
                Affiliation = CharacterAffiliationManager.Build(CharacterGroup.Human, null, null),
                Aggression = Aggression.AttackAnyone,
                Money = 0,
                IsLeader = true,
                IsShiny = false,
                IsBoss = false,
                CanPickUp = true,
                CanUseItem = true,
            };
        }

        public static CharacterMemento BuildCharacter(EnemyData data, Vector2Int spawnPosition, bool isSlept, bool isShiny, AffiliationMemento? affiliation = null, Id<IEntity>? id = null)
        {
            return new CharacterMemento
            {
                Name = isShiny ? "☆" + data.Name : data.Name,
                CharacterType = data.CharacterType,
                Behavior = data.Behavior,
                Status = CharacterStatusManager.Build(isShiny ? data.Hp * 3 : data.Hp, 0, isShiny ? 2 : 1, new Dictionary<Element, float>(), data.ElementDamageRateMultiplier, 8, data.MoveSpeed.ToWaitTime(), isSlept),
                Entity = Entity.Build(spawnPosition, EntityLayer.Middle),
                Direction = Direction8.Down,
                Skills = data.Skills.Select(x => CharacterSkill.Build(SpawnEffectSkill.Build(x.Skill), x.CoolTime)).ToArray(),
                LastSkill = new Option<SpawnEffectSkillMemento>(data.HasLastSkill ? SpawnEffectSkill.Build(data.LastSkill) : null),
                Inventory = new InventoryMemento
                {
                    Items = EnumerableExtension.CreateNewInstances<Option<ItemMemento>>(10).ToArray()
                },
                Affiliation = CharacterAffiliationManager.Build(data.Group, affiliation, id),
                Aggression = data.Aggression,
                Money = 0,
                IsLeader = false,
                IsShiny = isShiny,
                IsBoss = data.IsBoss,
                CanPickUp = data.CanPickUp,
                CanUseItem = data.CanUseItem
            };
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